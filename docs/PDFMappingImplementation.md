# PDF Highlight Mapping Implementation

These notes outline the steps taken to add PDF highlighting and field mapping using free tools in a Razor Pages project.

## Libraries
- **pdf.js** (Apache-2.0 license) renders PDFs client-side. Install it in a front-end build step using:
  ```bash
  npm install pdfjs-dist
  ```
  Copy `node_modules/pdfjs-dist/build/pdf.min.js`, `pdf.worker.min.js` and the `pdf_viewer.css` file to `wwwroot/lib/pdfjs/`.

## Backend Steps
1. Added a new `PdfSelection` model (`Models/PdfSelection.cs`) to capture the field name, page number, coordinates and selected text.
2. Created `PdfController` with actions to upload a PDF, display it for annotation and receive mapped selections.
3. Uploaded files are stored under `wwwroot/uploads` for temporary access when rendering.
4. Updated `Program.cs` so `UseHttpsRedirection` only runs when an HTTPS port is configured. This removes the runtime warning `Failed to determine the https port for redirect` when running in development.

## Frontend Steps
1. Added `Views/Pdf/Upload.cshtml` to let the user select and upload a PDF.
2. Added `Views/Pdf/Annotate.cshtml` which references `pdf.min.js`, `pdf.worker.min.js` and the viewer CSS from `wwwroot/lib/pdfjs` alongside a custom script.
3. Created `wwwroot/scripts/pdfmapper.js` that renders each PDF page and overlays a text layer so selections match the page layout exactly.
4. When the user releases the mouse over selected text, the script records the bounding box (X, Y, width, height), page number and text. It then prompts for a field mapping and shows the highlight in yellow using a `<mark>` element.
5. Clicking **Save Selections** sends all mapped selections back to `Pdf/SaveMapping` as JSON where they can be stored in your data model.

## Usage
1. Navigate to `/Pdf/Upload` to upload a PDF file.
2. After upload, the viewer opens where text can be highlighted and mapped.
3. Click **Save Selections** to send the data to the server for further processing.

This implementation uses only Apache-2.0 licensed pdf.js and standard ASP.NET Core components, satisfying commercial usage requirements without attribution.
