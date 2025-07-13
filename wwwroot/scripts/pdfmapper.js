// Basic PDF.js setup
const url = window.pdfFile;
const container = document.getElementById('viewerContainer');

const pdfjsLib = window['pdfjs-dist/build/pdf'];
pdfjsLib.GlobalWorkerOptions.workerSrc = '/lib/pdfjs/pdf.worker.min.js';

let pdfDoc = null;
const selections = [];

async function renderPage(num) {
    const page = await pdfDoc.getPage(num);
    const viewport = page.getViewport({scale:1.5});
    const canvas = document.createElement('canvas');
    canvas.width = viewport.width;
    canvas.height = viewport.height;
    container.appendChild(canvas);
    const ctx = canvas.getContext('2d');

    const renderTask = page.render({canvasContext: ctx, viewport});
    await renderTask.promise;
    const textContent = await page.getTextContent();
    pdfjsLib.renderTextLayer({
        textContent,
        container: canvas.parentElement,
        viewport,
        textDivs: []
    });
}

pdfjsLib.getDocument(url).promise.then(function(doc) {
    pdfDoc = doc;
    for(let i=1;i<=doc.numPages;i++)
        renderPage(i);
});

container.addEventListener('mouseup', () => {
    const selection = window.getSelection();
    if(selection && selection.toString().trim() !== '') {
        const range = selection.getRangeAt(0);
        const rect = range.getBoundingClientRect();
        const pageElement = range.startContainer.parentElement.closest('div');
        const pageNumber = Array.from(container.children).indexOf(pageElement) + 1;
        const pageRect = pageElement.getBoundingClientRect();
        selections.push({
            page: pageNumber,
            x: rect.left - pageRect.left,
            y: rect.top - pageRect.top,
            width: rect.width,
            height: rect.height,
            text: selection.toString()
        });
        const mark = document.createElement('mark');
        mark.style.backgroundColor = 'yellow';
        range.surroundContents(mark);
        const field = prompt('Map to field (amount/date/currency/etc):');
        if(field) {
            selections[selections.length-1].field = field;
        }
        selection.removeAllRanges();
    }
});

document.getElementById('saveSelectionBtn').addEventListener('click', () => {
    fetch('/Pdf/SaveMapping', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(selections)
    }).then(res => {
        if(res.ok) alert('Saved');
    });
});
