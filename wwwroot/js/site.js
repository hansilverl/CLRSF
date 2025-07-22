// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Scroll to results if anchor exists in URL or on page load
window.addEventListener('DOMContentLoaded', function () {
    var anchor = document.getElementById('results-anchor');
    if (anchor) {
        anchor.scrollIntoView({ behavior: 'smooth' });
    }
});
