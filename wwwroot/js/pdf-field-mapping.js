/**
 * PDF Field Mapping Application
 * Handles PDF loading, text selection, and field mapping functionality
 */

// Global variables for PDF handling
let currentFile = null;
let pdfViewer = null;
let selectedText = '';
let mappedFields = {
    date: '',
    amount: '',
    sourceCurrency: '',
    targetCurrency: '',
    bankRate: '',
    fees: ''
};

document.addEventListener('DOMContentLoaded', function() {
    
    // Initialize the PDF viewer
    initializePdfViewer();
    
    // File input change handler
    document.getElementById('fileInput').addEventListener('change', function(e) {
        const file = e.target.files[0];
        if (file) {
            currentFile = file;
            processSelectedFile(file);
        }
    });

    // Make entire upload area clickable
    document.getElementById('uploadArea').addEventListener('click', function(e) {
        // Don't trigger if clicking on buttons or form elements
        if (!e.target.closest('.btn') && 
            !e.target.closest('.btn-remove') && 
            !e.target.closest('button') &&
            !e.target.closest('input')) {
            document.getElementById('fileInput').click();
        }
    });
    
    // Handle buttons with data-action attributes
    document.addEventListener('click', function(e) {
        const action = e.target.dataset.action;
        
        switch(action) {
            case 'remove-file':
                removeFile();
                break;
            case 'process-file':
                processFile();
                break;
            case 'switch-to-manual':
                e.preventDefault();
                switchToManual();
                break;
            case 'clear-field':
                const field = e.target.dataset.field;
                if (field) {
                    clearSingleField(field);
                }
                break;
        }
    });
    
    // PDF Selector Overlay controls
    document.getElementById('closePdfSelector')?.addEventListener('click', hidePdfSelectorOverlay);
    
    // Close overlay when clicking outside
    document.getElementById('pdfSelectorOverlay')?.addEventListener('click', function(e) {
        if (e.target === this) {
            hidePdfSelectorOverlay();
        }
    });
    
    // Escape key to close overlay
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && document.getElementById('pdfSelectorOverlay').classList.contains('active')) {
            hidePdfSelectorOverlay();
        }
    });
    
    // Field mapping buttons
    document.querySelectorAll('.mapping-field-item').forEach(item => {
        item.addEventListener('click', function() {
            const field = this.dataset.field;
            if (selectedText && selectedText.trim()) {
                // Remove active state from all items
                document.querySelectorAll('.mapping-field-item').forEach(i => i.classList.remove('active'));
                // Add active state to clicked item
                this.classList.add('active');
                
                // Map the field value
                mapFieldValue(field, selectedText.trim());
                
                // Remove active state after a short delay
                setTimeout(() => {
                    this.classList.remove('active');
                }, 500);
            } else {
                // Show helpful message
                const selectedTextDiv = document.getElementById('selectedText');
                selectedTextDiv.innerHTML = '<span style="color: var(--warning-color);"><i class="bi bi-exclamation-circle"></i> Please select text from the PDF first</span>';
                selectedTextDiv.classList.add('scale-in');
                setTimeout(() => {
                    selectedTextDiv.classList.remove('scale-in');
                    if (!selectedText) {
                        selectedTextDiv.innerHTML = 'Click on words or drag to select text from the PDF';
                    }
                }, 2000);
            }
        });
    });

    // Action buttons
    document.getElementById('applyMappedValues')?.addEventListener('click', applyMappedValuesToForm);
    document.getElementById('clearMappings')?.addEventListener('click', clearAllMappings);
});

// Initialize PDF Viewer
function initializePdfViewer() {
    try {
        console.log('Initializing PDF viewer...');
        
        // Check if PDF.js is loaded
        if (typeof pdfjsLib === 'undefined') {
            console.error('PDF.js library not loaded. Retrying in 1 second...');
            setTimeout(initializePdfViewer, 1000);
            return;
        }
        
        // Check if the container exists
        const container = document.getElementById('nativePdfViewer');
        if (!container) {
            console.error('PDF viewer container not found');
            return;
        }
        
        // Check if PDFViewer is available
        if (typeof PDFViewer === 'undefined') {
            console.error('PDFViewer class not found. Make sure pdf-viewer.js is loaded.');
            return;
        }
        
        // Clean up any existing instance
        if (pdfViewer) {
            console.log('Cleaning up existing PDF viewer instance...');
            try {
                pdfViewer.destroy();
            } catch (destroyError) {
                console.warn('Error destroying existing PDF viewer:', destroyError);
            }
            pdfViewer = null;
        }
        
        console.log('PDF.js version:', pdfjsLib.version);
        
        // Initialize the PDF viewer
        pdfViewer = new PDFViewer('nativePdfViewer', {
            scale: 1.2
        });

        console.log('PDF viewer initialized successfully');

        // Verify the viewer was created properly
        if (!pdfViewer || !pdfViewer.container) {
            throw new Error('PDF viewer failed to initialize properly');
        }

        // Listen for text selection events
        document.getElementById('nativePdfViewer').addEventListener('textSelected', function(event) {
            selectedText = event.detail.text;
            console.log('Text selected:', selectedText);
            updateSelectedTextDisplay();
        });

        // Listen for word click events
        document.getElementById('nativePdfViewer').addEventListener('wordClicked', function(event) {
            selectedText = event.detail.word;
            console.log('Word clicked:', selectedText);
            updateSelectedTextDisplay();
        });

    } catch (error) {
        console.error('Failed to initialize PDF viewer:', error);
    }
}

// Process file function
function processFile() {
    console.log('processFile called, currentFile:', currentFile);
    
    if (!currentFile) {
        alert('Please select a file first');
        return;
    }
    
    const fileName = currentFile.name.toLowerCase();
    const uploadBtn = document.getElementById('uploadBtn');
    
    console.log('Processing file:', fileName);
    
    // Show loading state
    if (uploadBtn) {
        uploadBtn.innerHTML = '<i class="spinner-border spinner-border-sm me-2"></i>Processing...';
        uploadBtn.disabled = true;
    }
    
    if (fileName.endsWith('.pdf')) {
        console.log('File is PDF, showing overlay and loading...');
        // Show PDF selector overlay and load the PDF
        showPdfSelectorOverlay();
        loadPDFWithViewer(currentFile);
        
        // Reset button state
        setTimeout(() => {
            if (uploadBtn) {
                uploadBtn.innerHTML = '<i class="bi bi-upload me-2"></i>Process File';
                uploadBtn.disabled = false;
            }
        }, 500);
    } else {
        // Handle other file types (CSV, TXT) - for future implementation
        setTimeout(() => {
            alert('Currently only PDF files are supported for viewer functionality');
            if (uploadBtn) {
                uploadBtn.innerHTML = '<i class="bi bi-upload me-2"></i>Process File';
                uploadBtn.disabled = false;
            }
        }, 1000);
    }
}

// Load PDF with Viewer
async function loadPDFWithViewer(file) {
    console.log('loadPDFWithViewer called with file:', file);
    
    // Ensure we have a fresh PDF viewer instance
    if (!pdfViewer) {
        console.log('No PDF viewer instance found, reinitializing...');
        try {
            initializePdfViewer();
            // Wait a moment for initialization to complete
            await new Promise(resolve => setTimeout(resolve, 100));
        } catch (error) {
            console.error('Failed to reinitialize PDF viewer:', error);
            alert('Failed to initialize PDF viewer. Please refresh the page and try again.');
            return;
        }
    } else {
        // If we have an existing viewer, try to reinitialize it to ensure clean state
        console.log('Reinitializing existing PDF viewer for clean state...');
        try {
            // Clean way to reset the viewer - just reinitialize it
            initializePdfViewer();
            // Wait a moment for reinitialization to complete
            await new Promise(resolve => setTimeout(resolve, 200));
        } catch (error) {
            console.warn('Error reinitializing PDF viewer, creating new instance:', error);
            try {
                pdfViewer.destroy();
                pdfViewer = null;
                initializePdfViewer();
                await new Promise(resolve => setTimeout(resolve, 200));
            } catch (fallbackError) {
                console.error('Failed to create new PDF viewer instance:', fallbackError);
                alert('Failed to initialize PDF viewer. Please refresh the page and try again.');
                return;
            }
        }
    }
    
    if (!pdfViewer) {
        console.error('PDF viewer not initialized after reinitialize attempt');
        alert('PDF viewer not initialized. Please refresh the page and try again.');
        return;
    }

    try {
        console.log('Loading PDF with viewer...');
        await pdfViewer.loadPDF(file);
        console.log('PDF loaded successfully');
    } catch (error) {
        console.error('Error loading PDF:', error);
        alert('Error loading PDF: ' + error.message);
    }
}

// Show PDF selector overlay
function showPdfSelectorOverlay() {
    const overlay = document.getElementById('pdfSelectorOverlay');
    overlay.style.display = 'flex';
    // Trigger reflow
    overlay.offsetHeight;
    overlay.classList.add('active');
    document.body.style.overflow = 'hidden';
    
    // Force PDF viewer refresh after overlay is fully visible
    // Use multiple timeouts to ensure proper rendering with increased delays
    setTimeout(() => {
        if (pdfViewer && typeof pdfViewer.forceRefresh === 'function') {
            console.log('Overlay visible, forcing PDF refresh (first attempt)');
            pdfViewer.forceRefresh();
        }
    }, 150); // Slightly longer initial attempt
    
    setTimeout(() => {
        if (pdfViewer && typeof pdfViewer.forceRefresh === 'function') {
            console.log('Overlay visible, forcing PDF refresh (second attempt)');
            pdfViewer.forceRefresh();
        }
    }, 500); // After transition completes
    
    setTimeout(() => {
        if (pdfViewer && typeof pdfViewer.forceRefresh === 'function') {
            console.log('Overlay visible, forcing PDF refresh (final attempt)');
            pdfViewer.forceRefresh();
        }
    }, 1000); // Extra safety margin with longer delay
}

// Hide PDF selector overlay
function hidePdfSelectorOverlay() {
    const overlay = document.getElementById('pdfSelectorOverlay');
    overlay.classList.remove('active');
    document.body.style.overflow = '';
    setTimeout(() => {
        overlay.style.display = 'none';
        
        // Clear any text selections when overlay is hidden
        if (pdfViewer && typeof pdfViewer.clearSelection === 'function') {
            pdfViewer.clearSelection();
        }
        selectedText = '';
        updateSelectedTextDisplay();
    }, 300);
}

// Update selected text display
function updateSelectedTextDisplay() {
    const selectedTextDiv = document.getElementById('selectedText');
    if (selectedTextDiv) {
        if (selectedText && selectedText.trim()) {
            selectedTextDiv.innerHTML = `<span class="text-primary"><i class="bi bi-check-circle"></i> ${selectedText.substring(0, 100)}${selectedText.length > 100 ? '...' : ''}</span>`;
            selectedTextDiv.classList.add('has-text');
        } else {
            selectedTextDiv.innerHTML = 'Click on words or drag to select text from the PDF';
            selectedTextDiv.classList.remove('has-text');
        }
    }
}

// File upload drag and drop handlers
function handleDragOver(e) {
    e.preventDefault();
    const uploadArea = document.getElementById('uploadArea');
    uploadArea.style.borderColor = 'var(--clearshift-teal)';
    uploadArea.style.backgroundColor = 'rgba(74, 157, 168, 0.05)';
}

function handleDragLeave(e) {
    const uploadArea = document.getElementById('uploadArea');
    uploadArea.style.borderColor = '';
    uploadArea.style.backgroundColor = '';
}

function handleFileDrop(e) {
    e.preventDefault();
    const uploadArea = document.getElementById('uploadArea');
    uploadArea.style.borderColor = '';
    uploadArea.style.backgroundColor = '';
    
    const files = e.dataTransfer.files;
    if (files.length > 0) {
        const file = files[0];
        currentFile = file;
        processSelectedFile(file);
        
        // Update the file input
        const fileInput = document.getElementById('fileInput');
        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(file);
        fileInput.files = dataTransfer.files;
    }
}

function processSelectedFile(file) {
    const allowedTypes = ['.pdf', '.csv', '.txt'];
    const fileName = file.name.toLowerCase();
    const isValidType = allowedTypes.some(type => fileName.endsWith(type));
    
    const errorDiv = document.getElementById('uploadError');
    
    if (!isValidType) {
        errorDiv.textContent = 'Please upload a valid file (PDF, CSV, or TXT only)';
        errorDiv.style.display = 'block';
        removeFile();
        return;
    }
    
    // Check file size (limit to 10MB)
    const maxSize = 10 * 1024 * 1024; // 10MB in bytes
    if (file.size > maxSize) {
        errorDiv.textContent = 'File size must be less than 10MB';
        errorDiv.style.display = 'block';
        removeFile();
        return;
    }
    
    // Sanitize file name for display (security measure)
    const sanitizedFileName = sanitizeFileName(file.name);
    
    // Hide error and show file info
    errorDiv.style.display = 'none';
    document.querySelector('.file-name').textContent = sanitizedFileName;
    document.getElementById('fileInfo').style.display = 'block';
    document.getElementById('uploadBtn').style.display = 'inline-block';
    
    console.log('File selected:', sanitizedFileName, 'Size:', file.size, 'Type:', file.type);
    
    // Auto-process PDF files immediately
    if (fileName.endsWith('.pdf')) {
        console.log('Auto-processing PDF file...');
        setTimeout(() => {
            processFile();
        }, 100);
    }
}

function sanitizeFileName(fileName) {
    // Remove potentially dangerous characters and limit length
    let sanitized = fileName
        .replace(/[<>:"/\\|?*\x00-\x1f]/g, '') // Remove dangerous characters
        .replace(/\.\./g, '') // Remove directory traversal attempts
        .trim();
    
    // Limit length to prevent issues
    if (sanitized.length > 255) {
        const extension = sanitized.substring(sanitized.lastIndexOf('.'));
        const name = sanitized.substring(0, 255 - extension.length);
        sanitized = name + extension;
    }
    
    // Ensure it's not empty after sanitization
    if (!sanitized || sanitized === '') {
        sanitized = 'unnamed_file';
    }
    
    return sanitized;
}

function removeFile() {
    document.getElementById('fileInput').value = '';
    document.getElementById('fileInfo').style.display = 'none';
    document.getElementById('uploadError').style.display = 'none';
    document.getElementById('uploadBtn').style.display = 'none';
    hidePdfSelectorOverlay();
    currentFile = null;
    
    // Properly destroy the PDF viewer to prevent issues with subsequent loads
    if (pdfViewer) {
        try {
            console.log('Destroying PDF viewer on file removal...');
            pdfViewer.destroy();
            pdfViewer = null;
        } catch (error) {
            console.warn('Error destroying PDF viewer:', error);
            pdfViewer = null; // Force reset
        }
    }
    
    // Reset state
    selectedText = '';
    clearAllMappings();
}

// Map field value
function mapFieldValue(field, value) {
    let processedValue = value;
    
    // Process the value based on field type
    switch (field) {
        case 'date':
            processedValue = parseDate(value) || value;
            break;
        case 'amount':
        case 'bankRate':
        case 'fees':
            const numericValue = extractNumericValue(value);
            processedValue = numericValue !== null ? numericValue.toString() : value;
            break;
        case 'sourceCurrency':
        case 'targetCurrency':
            processedValue = extractCurrencyCode(value) || value;
            break;
    }
    
    // Store the mapped value
    mappedFields[field] = processedValue;
    
    // Update the field display
    const fieldElement = document.getElementById(`mapped${field.charAt(0).toUpperCase() + field.slice(1)}`);
    const fieldItem = document.querySelector(`[data-field="${field}"].mapping-field-item`);
    const fieldStatus = fieldItem?.querySelector('.field-status');
    
    if (fieldElement && fieldItem) {
        fieldElement.textContent = processedValue;
        fieldItem.classList.add('filled');
        
        if (fieldStatus) {
            fieldStatus.textContent = 'Mapped';
            fieldStatus.classList.remove('empty');
            fieldStatus.classList.add('filled');
        }
    }
    
    // Update apply button state
    updateApplyButtonState();
    
    // Show success feedback
    showFieldMappedFeedback(field);
    
    // Clear selections after mapping
    setTimeout(() => {
        if (pdfViewer) {
            pdfViewer.clearSelection();
        }
        selectedText = '';
        updateSelectedTextDisplay();
    }, 1000);
}

// Clear a single field
function clearSingleField(field) {
    mappedFields[field] = '';
    
    const fieldElement = document.getElementById(`mapped${field.charAt(0).toUpperCase() + field.slice(1)}`);
    const fieldItem = document.querySelector(`[data-field="${field}"].mapping-field-item`);
    const fieldStatus = fieldItem?.querySelector('.field-status');
    
    if (fieldElement && fieldItem) {
        fieldElement.textContent = 'Click to map selected text';
        fieldItem.classList.remove('filled', 'active');
        
        if (fieldStatus) {
            fieldStatus.textContent = 'Empty';
            fieldStatus.classList.remove('filled');
            fieldStatus.classList.add('empty');
        }
    }
    
    updateApplyButtonState();
}

// Show field mapped feedback
function showFieldMappedFeedback(field) {
    const fieldItem = document.querySelector(`[data-field="${field}"].mapping-field-item`);
    if (fieldItem) {
        const fieldLabel = fieldItem.querySelector('.field-label');
        const originalHTML = fieldLabel.innerHTML;
        
        fieldLabel.innerHTML = '<i class="bi bi-check-circle field-icon" style="color: var(--success-color);"></i>Mapped Successfully!';
        fieldItem.classList.add('scale-in');
        
        setTimeout(() => {
            fieldLabel.innerHTML = originalHTML;
            fieldItem.classList.remove('scale-in');
        }, 1500);
    }
}

// Update apply button state
function updateApplyButtonState() {
    const applyButton = document.getElementById('applyMappedValues');
    const hasMappedValues = Object.values(mappedFields).some(value => value && value.trim());
    
    if (applyButton) {
        applyButton.disabled = !hasMappedValues;
    }
}

// Clear all mappings
function clearAllMappings() {
    // Reset mapped fields
    Object.keys(mappedFields).forEach(field => {
        mappedFields[field] = '';
        
        const fieldElement = document.getElementById(`mapped${field.charAt(0).toUpperCase() + field.slice(1)}`);
        const fieldItem = document.querySelector(`[data-field="${field}"].mapping-field-item`);
        const fieldStatus = fieldItem?.querySelector('.field-status');
        
        if (fieldElement && fieldItem) {
            fieldElement.textContent = 'Click to map selected text';
            fieldItem.classList.remove('filled', 'active');
            
            if (fieldStatus) {
                fieldStatus.textContent = 'Empty';
                fieldStatus.classList.remove('filled');
                fieldStatus.classList.add('empty');
            }
        }
    });
    
    // Clear selected text
    selectedText = '';
    const selectedTextDiv = document.getElementById('selectedText');
    selectedTextDiv.textContent = 'Click on words or drag to select text from the PDF';
    selectedTextDiv.classList.remove('has-text');
    
    // Clear PDF selection
    if (pdfViewer) {
        pdfViewer.clearSelection();
    }
    
    // Update apply button
    updateApplyButtonState();
}

function applyMappedValuesToForm() {
    // Switch to manual tab
    switchToManual();
    
    // Hide the PDF overlay
    hidePdfSelectorOverlay();
    
    // Apply mapped values to form fields after a short delay
    setTimeout(() => {
        if (mappedFields.date) {
            const dateField = document.querySelector('input[name="Date"]');
            if (dateField) {
                // Try to parse and format the date
                const parsedDate = parseDate(mappedFields.date);
                if (parsedDate) {
                    dateField.value = parsedDate;
                }
            }
        }
        
        if (mappedFields.amount) {
            const amountDisplay = document.getElementById('AmountDisplay');
            const amountHidden = document.getElementById('Amount');
            if (amountDisplay && amountHidden) {
                // Extract numeric value
                const numericAmount = extractNumericValue(mappedFields.amount);
                if (numericAmount) {
                    amountHidden.value = numericAmount;
                    amountDisplay.value = formatNumberWithCommas(numericAmount);
                }
            }
        }
        
        if (mappedFields.sourceCurrency) {
            const sourceField = document.querySelector('select[name="SourceCurrency"]');
            if (sourceField) {
                // Try to match currency code
                const currencyCode = extractCurrencyCode(mappedFields.sourceCurrency);
                if (currencyCode) {
                    sourceField.value = currencyCode;
                }
            }
        }
        
        if (mappedFields.targetCurrency) {
            const targetField = document.querySelector('select[name="TargetCurrency"]');
            if (targetField) {
                const currencyCode = extractCurrencyCode(mappedFields.targetCurrency);
                if (currencyCode) {
                    targetField.value = currencyCode;
                }
            }
        }
        
        if (mappedFields.bankRate) {
            const rateField = document.querySelector('input[name="BankRate"]');
            if (rateField) {
                const numericRate = extractNumericValue(mappedFields.bankRate);
                if (numericRate) {
                    rateField.value = numericRate;
                }
            }
        }
        
        if (mappedFields.fees) {
            const feesField = document.querySelector('input[name="BankFees"]');
            if (feesField) {
                const numericFees = extractNumericValue(mappedFields.fees);
                if (numericFees) {
                    feesField.value = numericFees;
                }
            }
        }
        
        alert('Values applied to form successfully!');
    }, 500);
}

// Utility functions
function formatNumberWithCommas(num) {
    let cleanNum = num.toString().replace(/[^\d.]/g, '');
    let parts = cleanNum.split('.');
    parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',');
    return parts.join('.');
}

function parseDate(dateStr) {
    if (!dateStr || typeof dateStr !== 'string') {
        return null;
    }
    
    // Clean up the date string
    const cleanDateStr = dateStr.trim();
    console.log('parseDate called with:', cleanDateStr);
    
    // Try to parse various date formats
    const patterns = [
        // MM/DD/YYYY or DD/MM/YYYY format 
        {
            regex: /^(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{4})$/,
            handler: (match) => {
                const first = parseInt(match[1]);
                const second = parseInt(match[2]);
                const year = parseInt(match[3]);
                
                let month, day;
                
                // Check if this looks like DD/MM/YYYY (first number > 12 indicates day)
                if (first > 12) {
                    // This is DD/MM/YYYY
                    day = match[1].padStart(2, '0');
                    month = match[2].padStart(2, '0');
                    console.log('Detected DD/MM/YYYY format:', `${year}-${month}-${day}`);
                } else if (second > 12) {
                    // This is MM/DD/YYYY (second number > 12 indicates day)
                    month = match[1].padStart(2, '0');
                    day = match[2].padStart(2, '0');
                    console.log('Detected MM/DD/YYYY format:', `${year}-${month}-${day}`);
                } else {
                    // Ambiguous case - default to MM/DD/YYYY since example shows 09/10/2022 (September 10th)
                    month = match[1].padStart(2, '0');
                    day = match[2].padStart(2, '0');
                    console.log('Ambiguous date, assuming MM/DD/YYYY format:', `${year}-${month}-${day}`);
                }
                
                const result = `${year}-${month}-${day}`;
                console.log('parseDate returning:', result);
                return result;
            }
        },

        // YYYY/MM/DD or YYYY-MM-DD format
        {
            regex: /^(\d{4})[\/\-](\d{1,2})[\/\-](\d{1,2})$/,
            handler: (match) => {
                const year = match[1];
                const month = match[2].padStart(2, '0');
                const day = match[3].padStart(2, '0');
                return `${year}-${month}-${day}`;
            }
        },
        // Month names (e.g., "September 10, 2022" or "10 Sep 2022")
        {
            regex: /(\w+)\s+(\d{1,2}),?\s+(\d{4})|(\d{1,2})\s+(\w+)\s+(\d{4})/,
            handler: (match) => {
                const monthNames = {
                    'january': '01', 'jan': '01', 'february': '02', 'feb': '02', 'march': '03', 'mar': '03',
                    'april': '04', 'apr': '04', 'may': '05', 'june': '06', 'jun': '06',
                    'july': '07', 'jul': '07', 'august': '08', 'aug': '08', 'september': '09', 'sep': '09',
                    'october': '10', 'oct': '10', 'november': '11', 'nov': '11', 'december': '12', 'dec': '12'
                };
                
                let month, day, year;
                if (match[1]) {
                    // "Month day, year" format
                    month = monthNames[match[1].toLowerCase()];
                    day = match[2].padStart(2, '0');
                    year = match[3];
                } else {
                    // "day Month year" format
                    day = match[4].padStart(2, '0');
                    month = monthNames[match[5].toLowerCase()];
                    year = match[6];
                }
                
                if (month) {
                    return `${year}-${month}-${day}`;
                }
                return null;
            }
        },
        // Dot separated format (DD.MM.YYYY - common in Europe)
        {
            regex: /^(\d{1,2})\.(\d{1,2})\.(\d{4})$/,
            handler: (match) => {
                const day = match[1].padStart(2, '0');
                const month = match[2].padStart(2, '0');
                const year = match[3];
                return `${year}-${month}-${day}`;
            }
        }
    ];
    
    // Try each pattern
    for (const pattern of patterns) {
        const match = cleanDateStr.match(pattern.regex);
        if (match) {
            try {
                const result = pattern.handler(match);
                if (result) {
                    // Validate the resulting date
                    const date = new Date(result);
                    if (!isNaN(date.getTime()) && date.getFullYear() > 1900 && date.getFullYear() < 2100) {
                        return result;
                    }
                }
            } catch (e) {
                console.warn('Error parsing date with pattern:', pattern.regex, e);
            }
        }
    }
    
    // If no pattern matches, try JavaScript's Date.parse as a fallback
    try {
        const date = new Date(cleanDateStr);
        if (!isNaN(date.getTime()) && date.getFullYear() > 1900 && date.getFullYear() < 2100) {
            const year = date.getFullYear();
            const month = (date.getMonth() + 1).toString().padStart(2, '0');
            const day = date.getDate().toString().padStart(2, '0');
            const result = `${year}-${month}-${day}`;
            console.log('parseDate fallback returning:', result);
            return result;
        }
    } catch (e) {
        console.warn('Error with Date.parse fallback:', e);
    }
    
    console.log('parseDate returning null for:', cleanDateStr);
    return null;
}

function extractNumericValue(str) {
    // Extract numeric value, handling commas and currency symbols
    const match = str.replace(/[^\d.,]/g, '').replace(/,/g, '');
    return match ? parseFloat(match) : null;
}

function extractCurrencyCode(str) {
    // Common currency codes
    const currencies = ['USD', 'EUR', 'GBP', 'ILS', 'JPY', 'CAD', 'AUD', 'CHF'];
    const upperStr = str.toUpperCase();
    
    for (const currency of currencies) {
        if (upperStr.includes(currency)) {
            return currency;
        }
    }
    return null;
}

function switchToManual() {
    // Find and click the manual input tab button
    const manualTab = document.querySelector('[data-tab="manual"]');
    if (manualTab) {
        manualTab.click();
    } else {
        // Alternative method if tab structure is different
        const tabButtons = document.querySelectorAll('.tab-button');
        tabButtons.forEach(function(button) {
            if (button.textContent.includes('Manual') || button.textContent.includes('manual')) {
                button.click();
            }
        });
    }
}

// Prevent default drag behaviors on the document
document.addEventListener('dragover', function(e) {
    e.preventDefault();
});

document.addEventListener('drop', function(e) {
    e.preventDefault();
});

// Make these functions globally available
window.handleDragOver = handleDragOver;
window.handleDragLeave = handleDragLeave;
window.handleFileDrop = handleFileDrop;
window.processFile = processFile;
window.removeFile = removeFile;
window.clearSingleField = clearSingleField;
window.switchToManual = switchToManual;
