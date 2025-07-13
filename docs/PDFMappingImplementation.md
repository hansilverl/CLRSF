# PDF Highlight Mapping Implementation

These notes outline the steps taken to add PDF highlighting and field mapping using free tools in a Razor Pages project.

## Libraries
- **pdf.js** (Apache-2.0 license) is used to render PDFs client-side. Install via `npm install pdfjs-dist` and copy the `build` directory to `wwwroot/lib/pdfjs`.

## Backend Steps
1. Added a new `PdfSelection` model (`Models/PdfSelection.cs`) to capture field name, page, coordinates and selected text.
2. Created `PdfController` with actions to upload a PDF, display it for annotation and receive mapped selections.
3. Uploaded files are stored under `wwwroot/uploads` for temporary access when rendering.

## Frontend Steps
1. Added `Views/Pdf/Upload.cshtml` to let the user select and upload a PDF.
2. Added `Views/Pdf/Annotate.cshtml` which references pdf.js and a custom script.
3. Created `wwwroot/js/pdfmapper.js` that renders the PDF, allows text selection and highlights it. Each selection prompts the user to map the text to a field and then stores the coordinates along with the page number.
4. When the user clicks **Save Selections**, the selections array is posted back to the server via `Pdf/SaveMapping`.

## Usage
1. Navigate to `/Pdf/Upload` to upload a PDF file.
2. After upload, the viewer opens where text can be highlighted and mapped.
3. Click **Save Selections** to send the data to the server for further processing.

This implementation uses only Apache-2.0 licensed pdf.js and standard ASP.NET Core components, satisfying commercial usage requirements without attribution.
