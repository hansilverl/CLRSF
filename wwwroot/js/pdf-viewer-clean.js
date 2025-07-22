/**
 * Clean PDF Viewer using PDF.js complete viewer infrastructure
 * This avoids text overlay issues by using PDF.js native rendering
 */

class CleanPDFViewer {
    constructor(containerId, options = {}) {
        this.containerId = containerId;
        this.container = document.getElementById(containerId);
        this.options = {
            scale: 1.2,
            ...options
        };

        // State
        this.pdfDoc = null;
        this.currentPage = 1;
        this.totalPages = 0;
        this.scale = this.options.scale;
        this.selectedText = '';
        this.pdfViewer = null;
        this.eventBus = null;

        this.init();
    }

    init() {
        if (!this.container) {
            throw new Error(`Container with ID "${this.containerId}" not found`);
        }

        // Set up PDF.js worker
        if (typeof pdfjsLib !== 'undefined') {
            pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';
        }

        this.createHTML();
        this.bindEvents();
        this.initializePDFViewer();
    }

    createHTML() {
        this.container.innerHTML = `
            <div class="clean-pdf-viewer">
                <!-- Controls -->
                <div class="pdf-controls">
                    <div class="nav-controls">
                        <button type="button" id="prevPage" class="control-btn">
                            <i class="bi bi-chevron-left"></i>
                        </button>
                        <div class="page-info">
                            <span id="pageNum">1</span> / <span id="pageCount">0</span>
                        </div>
                        <button type="button" id="nextPage" class="control-btn">
                            <i class="bi bi-chevron-right"></i>
                        </button>
                    </div>
                    <div class="zoom-controls">
                        <button type="button" id="zoomOut" class="control-btn">
                            <i class="bi bi-zoom-out"></i>
                        </button>
                        <div class="zoom-display">
                            <span id="zoomLevel">100%</span>
                        </div>
                        <button type="button" id="zoomIn" class="control-btn">
                            <i class="bi bi-zoom-in"></i>
                        </button>
                    </div>
                </div>
                <!-- Loading indicator -->
                <div id="loadingIndicator" class="loading-container">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="mt-3">Loading PDF...</p>
                </div>

                <!-- PDF Content using PDF.js viewer -->
                <div id="pdfContent" class="pdf-content" style="display: none;">
                    <div id="viewerContainer" class="viewer-container">
                        <div id="viewer" class="pdfViewer"></div>
                    </div>
                </div>
            </div>
        `;
    }

    initializePDFViewer() {
        if (typeof pdfjsViewer === 'undefined') {
            console.warn('PDF.js viewer not available, using fallback');
            return;
        }

        // Create event bus for PDF.js viewer
        this.eventBus = new pdfjsViewer.EventBus();
        // Listen for page change events to update page number UI
        this.eventBus.on('pagechange', (evt) => {
            // evt.pageNumber is 1-based
            this.currentPage = evt.pageNumber;
            const pageNumElem = this.container.querySelector('#pageNum');
            if (pageNumElem) {
                pageNumElem.textContent = evt.pageNumber;
            }
            // Optionally update pageCount if needed
            const pageCountElem = this.container.querySelector('#pageCount');
            if (pageCountElem && this.pdfViewer && this.pdfViewer.pagesCount) {
                pageCountElem.textContent = this.pdfViewer.pagesCount;
            }
        });
        
        // Create PDF viewer - ensure container is positioned correctly for PDF.js
        const viewerContainer = this.container.querySelector('#viewerContainer');
        if (viewerContainer) {
            // PDF.js requires the container to be absolutely positioned
            viewerContainer.style.position = 'absolute';
            viewerContainer.style.top = '0';
            viewerContainer.style.left = '0';
            viewerContainer.style.right = '0';
            viewerContainer.style.bottom = '0';
            viewerContainer.style.overflow = 'auto';
        }
        
        this.pdfViewer = new pdfjsViewer.PDFViewer({
            container: viewerContainer,
            viewer: this.container.querySelector('#viewer'),
            eventBus: this.eventBus,
            textLayerMode: 2, // Enable text selection
            annotationMode: 1,
            removePageBorders: true
        });

        // Link event bus to viewer
        this.eventBus.on('pagesinit', () => {
            this.pdfViewer.currentScaleValue = this.scale;
            console.log('Pages initialized, scale set to:', this.scale);
            
            // Force immediate updates when pages are initialized
            requestAnimationFrame(() => {
                if (this.pdfViewer) {
                    this.pdfViewer.update();
                    if (this.pdfViewer.forceRendering) {
                        this.pdfViewer.forceRendering();
                    }
                }
            });
        });

        this.eventBus.on('pagerendered', (evt) => {
            console.log('Page rendered:', evt.pageNumber);
        });

        this.eventBus.on('textlayerrendered', (evt) => {
            console.log('Text layer rendered for page:', evt.pageNumber);
        });
        
        // Add intersection observer to handle visibility changes
        this.setupVisibilityObserver();
    }

    bindEvents() {
        this.container.querySelector('#prevPage').addEventListener('click', () => this.prevPage());
        this.container.querySelector('#nextPage').addEventListener('click', () => this.nextPage());
        this.container.querySelector('#zoomIn').addEventListener('click', () => this.zoomIn());
        this.container.querySelector('#zoomOut').addEventListener('click', () => this.zoomOut());

        // Text selection handling
        document.addEventListener('selectionchange', () => this.handleTextSelection());
    }

    async loadPDF(source) {
        const loadingDiv = this.container.querySelector('#loadingIndicator');
        const contentDiv = this.container.querySelector('#pdfContent');
        
        // Make content div visible immediately to allow proper rendering
        contentDiv.style.display = 'flex';
        contentDiv.style.visibility = 'visible';
        contentDiv.style.opacity = '0';
        loadingDiv.style.display = 'flex';

        try {
            console.log('Loading PDF with clean viewer...');

            let arrayBuffer;
            if (source instanceof File) {
                arrayBuffer = await this.fileToArrayBuffer(source);
            } else if (typeof source === 'string') {
                const response = await fetch(source);
                arrayBuffer = await response.arrayBuffer();
            } else {
                throw new Error('Invalid PDF source');
            }

            // Validate PDF file before processing
            if (!arrayBuffer || arrayBuffer.byteLength === 0) {
                throw new Error('Empty or invalid PDF file');
            }

            // Check for PDF header
            const firstBytes = new Uint8Array(arrayBuffer.slice(0, 4));
            const pdfHeader = String.fromCharCode(...firstBytes);
            if (!pdfHeader.startsWith('%PDF')) {
                throw new Error('File does not appear to be a valid PDF');
            }

            const typedArray = new Uint8Array(arrayBuffer);
            
            // Enhanced PDF loading with better error handling
            const loadingTask = pdfjsLib.getDocument({
                data: typedArray,
                disableAutoFetch: false,
                disableStream: false,
                isEvalSupported: false,
                maxImageSize: 1024 * 1024 * 10, // 10MB max image size
                cMapPacked: true
            });

            this.pdfDoc = await loadingTask.promise;
            this.totalPages = this.pdfDoc.numPages;

            console.log(`PDF loaded: ${this.totalPages} pages`);

            // Update UI
            this.container.querySelector('#pageCount').textContent = this.totalPages;

            // Ensure the viewer container is properly positioned
            const viewerContainer = this.container.querySelector('#viewerContainer');
            if (viewerContainer) {
                viewerContainer.style.position = 'absolute';
                viewerContainer.style.top = '0';
                viewerContainer.style.left = '0';
                viewerContainer.style.right = '0';
                viewerContainer.style.bottom = '0';
                viewerContainer.style.overflow = 'auto';
            }

            // Defensive: reset viewer before loading new document
            if (this.pdfViewer) {
                // Complete cleanup and recreation approach
                try {
                    console.log('Cleaning up existing PDF viewer...');
                    
                    // Destroy the current viewer completely to avoid state issues
                    if (this.pdfViewer._annotationEditorUIManager) {
                        try {
                            if (typeof this.pdfViewer._annotationEditorUIManager.destroy === 'function') {
                                this.pdfViewer._annotationEditorUIManager.destroy();
                            }
                        } catch (annotErr) {
                            console.warn('Error destroying annotation manager:', annotErr);
                        }
                        this.pdfViewer._annotationEditorUIManager = null;
                    }
                    
                    // Clear the viewer container
                    const viewerElement = this.container.querySelector('#viewer');
                    if (viewerElement) {
                        viewerElement.innerHTML = '';
                    }
                    
                    // Set pdfViewer to null to force recreation
                    this.pdfViewer = null;
                    
                } catch (recreateErr) {
                    console.warn('Error during PDF viewer cleanup:', recreateErr);
                    this.pdfViewer = null; // Force recreation
                }
            }
            
            // Always recreate the PDF viewer to ensure clean state
            try {
                const viewerContainer = this.container.querySelector('#viewerContainer');
                if (!viewerContainer) {
                    throw new Error('The viewerContainer element not found.');
                }
                
                // Ensure the container is properly set up for PDF.js requirements
                viewerContainer.style.position = 'absolute';
                viewerContainer.style.top = '0';
                viewerContainer.style.left = '0';
                viewerContainer.style.right = '0';
                viewerContainer.style.bottom = '0';
                viewerContainer.style.overflow = 'auto';
                
                this.pdfViewer = new pdfjsViewer.PDFViewer({
                    container: viewerContainer,
                    viewer: this.container.querySelector('#viewer'),
                    eventBus: this.eventBus,
                    textLayerMode: 2, // Enable text selection
                    annotationMode: 1,
                    removePageBorders: true
                });
                
                console.log('PDF viewer recreated, setting document...');
                
                this.pdfViewer.setDocument(this.pdfDoc);
            } catch (setDocErr) {
                console.error('Failed to load PDF in viewer:', setDocErr);
                throw new Error('Failed to load PDF in viewer: ' + setDocErr.message);
            }

            // Wait for pages to initialize with timeout
            await Promise.race([
                new Promise(resolve => {
                    this.eventBus.on('pagesinit', resolve, { once: true });
                }),
                new Promise(resolve => setTimeout(resolve, 5000)) // Increased timeout to 5 seconds
            ]);

            this.currentPage = 1;
            this.container.querySelector('#pageNum').textContent = this.currentPage;
            this.updateControls();

            // Force immediate render and update
            this.pdfViewer.update();
            if (this.pdfViewer.forceRendering) {
                this.pdfViewer.forceRendering();
            }

            // Wait a bit longer for initial render to complete
            await new Promise(resolve => setTimeout(resolve, 500));

            // Show content with smooth transition
            loadingDiv.style.display = 'none';
            contentDiv.style.transition = 'opacity 0.3s ease';

            // Fade in the content
            requestAnimationFrame(() => {
                contentDiv.style.opacity = '1';
            });

        } catch (error) {
            console.error('Error loading PDF:', error);
            
            let errorMessage = 'Error loading PDF';
            let errorDetail = error.message;
            
            // Provide more user-friendly error messages
            if (error.message.includes('Invalid PDF structure') || error.message.includes('Bad FCHECK')) {
                errorMessage = 'PDF file appears to be corrupted';
                errorDetail = 'Please try a different PDF file or re-save the original.';
            } else if (error.message.includes('container') && error.message.includes('positioned')) {
                errorMessage = 'PDF viewer positioning error';
                errorDetail = 'Please refresh the page and try again.';
            } else if (error.message.includes('FormatError')) {
                errorMessage = 'PDF format error';
                errorDetail = 'This PDF file may be damaged or use an unsupported format.';
            }
            
            loadingDiv.innerHTML = `
                <div class="text-danger text-center">
                    <i class="bi bi-exclamation-triangle"></i>
                    <p class="mt-2">${errorMessage}</p>
                    <small>${errorDetail}</small>
                </div>
            `;
            // Reset content div visibility on error
            contentDiv.style.display = 'none';
            contentDiv.style.visibility = 'visible';
        }
    }

    setupVisibilityObserver() {
        if (!this.container || typeof IntersectionObserver === 'undefined') {
            return;
        }

        const contentDiv = this.container.querySelector('#pdfContent');
        if (!contentDiv) return;

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && this.pdfViewer && this.pdfDoc) {
                    console.log('PDF viewer became visible, forcing update');
                    // Use requestAnimationFrame for better timing
                    requestAnimationFrame(() => {
                        this.forceRefresh();
                    });
                }
            });
        }, {
            threshold: 0.01, // Lower threshold for more sensitive detection
            rootMargin: '10px' // Add some margin for early detection
        });

        observer.observe(contentDiv);
        this.visibilityObserver = observer;
        
        // Also observe the main container for changes
        const mainContainer = this.container;
        if (mainContainer !== contentDiv) {
            const containerObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting && this.pdfViewer && this.pdfDoc) {
                        console.log('PDF container became visible, forcing update');
                        requestAnimationFrame(() => {
                            this.forceRefresh();
                        });
                    }
                });
            }, {
                threshold: 0.01,
                rootMargin: '10px'
            });
            
            containerObserver.observe(mainContainer);
            this.containerObserver = containerObserver;
        }
    }

    forceRefresh() {
        if (!this.pdfViewer || !this.pdfDoc) {
            return;
        }

        try {
            console.log('Force refreshing PDF viewer');
            
            // Ensure container positioning is correct before refresh
            const viewerContainer = this.container.querySelector('#viewerContainer');
            if (viewerContainer) {
                // Reset positioning to ensure PDF.js requirements are met
                viewerContainer.style.position = 'absolute';
                viewerContainer.style.top = '0';
                viewerContainer.style.left = '0';
                viewerContainer.style.right = '0';
                viewerContainer.style.bottom = '0';
                viewerContainer.style.overflow = 'auto';
            }
            
            // Multiple approaches to ensure proper rendering
            requestAnimationFrame(() => {
                // Force container resize detection
                if (this.pdfViewer.updateScaleAndCanvas) {
                    this.pdfViewer.updateScaleAndCanvas();
                }
                
                this.pdfViewer.update();
                
                // Force re-render of current pages
                if (this.pdfViewer.forceRendering) {
                    this.pdfViewer.forceRendering();
                }
                
                // Update scale to trigger refresh
                const currentScale = this.pdfViewer.currentScale;
                this.pdfViewer.currentScaleValue = currentScale;
                
                // Force another update after a short delay
                setTimeout(() => {
                    this.pdfViewer.update();
                    
                    // Additional refresh attempt
                    if (this.pdfViewer.scrollPageIntoView) {
                        this.pdfViewer.scrollPageIntoView({ pageNumber: this.currentPage });
                    }
                }, 100);
            });
            
        } catch (error) {
            console.warn('Error during force refresh:', error);
        }
    }

    handleTextSelection() {
        const selection = window.getSelection();
        const selectedText = selection.toString().trim();
        
        if (selectedText && selectedText !== this.selectedText) {
            this.selectedText = selectedText;
            console.log('Text selected from PDF:', selectedText);
            
            // Dispatch custom event
            this.container.dispatchEvent(new CustomEvent('textSelected', {
                detail: { text: selectedText }
            }));
        } else if (!selectedText) {
            this.selectedText = '';
        }
    }

    prevPage() {
        if (this.pdfViewer && this.pdfViewer.currentPageNumber > 1) {
            this.pdfViewer.currentPageNumber--;
            this.currentPage = this.pdfViewer.currentPageNumber;
            this.container.querySelector('#pageNum').textContent = this.currentPage;
            this.updateControls();
        }
    }

    nextPage() {
        if (this.pdfViewer && this.pdfViewer.currentPageNumber < this.totalPages) {
            this.pdfViewer.currentPageNumber++;
            this.currentPage = this.pdfViewer.currentPageNumber;
            this.container.querySelector('#pageNum').textContent = this.currentPage;
            this.updateControls();
        }
    }

    zoomIn() {
        this.scale = Math.min(3.0, this.scale + 0.2);
        this.updateZoomDisplay();
        
        if (this.pdfViewer) {
            this.pdfViewer.currentScaleValue = this.scale;
        }
    }

    zoomOut() {
        this.scale = Math.max(0.5, this.scale - 0.2);
        this.updateZoomDisplay();
        
        if (this.pdfViewer) {
            this.pdfViewer.currentScaleValue = this.scale;
        }
    }

    updateZoomDisplay() {
        const zoomPercent = Math.round(this.scale * 100);
        this.container.querySelector('#zoomLevel').textContent = `${zoomPercent}%`;
    }

    updateControls() {
        const prevBtn = this.container.querySelector('#prevPage');
        const nextBtn = this.container.querySelector('#nextPage');
        
        prevBtn.disabled = this.currentPage <= 1;
        nextBtn.disabled = this.currentPage >= this.totalPages;
    }

    fileToArrayBuffer(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = (e) => resolve(e.target.result);
            reader.onerror = reject;
            reader.readAsArrayBuffer(file);
        });
    }

    getSelectedText() {
        return this.selectedText;
    }

    clearSelection() {
        this.selectedText = '';
        
        // Clear browser text selection too
        if (window.getSelection) {
            window.getSelection().removeAllRanges();
        }
    }

    destroy() {
        try {
            // Clean up PDF viewer first
            if (this.pdfViewer) {
                try {
                    if (this.pdfViewer._annotationEditorUIManager) {
                        if (typeof this.pdfViewer._annotationEditorUIManager.destroy === 'function') {
                            this.pdfViewer._annotationEditorUIManager.destroy();
                        }
                        this.pdfViewer._annotationEditorUIManager = null;
                    }
                    // Clear the viewer
                    const viewerElement = this.container ? this.container.querySelector('#viewer') : null;
                    if (viewerElement) {
                        viewerElement.innerHTML = '';
                    }
                } catch (pdfViewerErr) {
                    console.warn('Error cleaning up PDF viewer:', pdfViewerErr);
                }
                this.pdfViewer = null;
            }
            
            // Clean up event bus
            if (this.eventBus) {
                try {
                    this.eventBus.destroy();
                } catch (eventBusErr) {
                    console.warn('Error destroying event bus:', eventBusErr);
                }
                this.eventBus = null;
            }
            
            // Clean up observers
            if (this.visibilityObserver) {
                try {
                    this.visibilityObserver.disconnect();
                } catch (obsErr) {
                    console.warn('Error disconnecting visibility observer:', obsErr);
                }
                this.visibilityObserver = null;
            }
            
            if (this.containerObserver) {
                try {
                    this.containerObserver.disconnect();
                } catch (obsErr) {
                    console.warn('Error disconnecting container observer:', obsErr);
                }
                this.containerObserver = null;
            }
            
            // Clean up PDF document
            if (this.pdfDoc) {
                try {
                    this.pdfDoc.destroy();
                } catch (docErr) {
                    console.warn('Error destroying PDF document:', docErr);
                }
                this.pdfDoc = null;
            }
            
            // Clear container content
            if (this.container) {
                this.container.innerHTML = '';
            }
            
            // Reset state
            this.currentPage = 1;
            this.totalPages = 0;
            this.selectedText = '';
            
        } catch (error) {
            console.error('Error during CleanPDFViewer destruction:', error);
        }
    }
}

// Export for use
window.CleanPDFViewer = CleanPDFViewer;
