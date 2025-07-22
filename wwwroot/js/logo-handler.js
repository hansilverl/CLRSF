/**
 * ClearShift Logo Handler
 * Handles logo fallback functionality
 */

document.addEventListener('DOMContentLoaded', function() {
    // Handle navbar logo fallback
    const navbarLogo = document.querySelector('.navbar-logo');
    if (navbarLogo) {
        navbarLogo.addEventListener('error', function() {
            this.style.display = 'none';
            const fallbackIcon = this.nextElementSibling;
            if (fallbackIcon) {
                fallbackIcon.style.display = 'inline';
            }
        });
    }

    // Handle footer logo fallback
    const footerLogo = document.querySelector('.footer-logo');
    if (footerLogo) {
        footerLogo.addEventListener('error', function() {
            this.style.display = 'none';
            const fallbackIcon = this.nextElementSibling;
            if (fallbackIcon) {
                fallbackIcon.style.display = 'inline';
            }
        });
    }
});
