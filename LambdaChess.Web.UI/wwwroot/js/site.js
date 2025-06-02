// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// site.js - Enhanced with UI/UX improvements

// Wait for document ready
$(document).ready(function () {
    // Initialize tooltips
    initTooltips();

    // Add transition effect on page load
    $('main').addClass('fade-in');

    // Add smooth scrolling to all links
    initSmoothScrolling();

    // Initialize toast notifications
    initToastNotifications();

    // Initialize dark mode toggler
    initDarkModeToggle();

    // Add accessibility improvements
    enhanceAccessibility();

    // Handle form validations with better UX
    enhanceFormValidation();

    // Handle AJAX forms with better feedback
    handleAjaxForms();

    // Handle active navigation highlighting
    highlightActiveNavigation();

    // Add responsive behaviors
    addResponsiveBehaviors();
});

// Initialize tooltips across the site
function initTooltips() {
    $('[data-bs-toggle="tooltip"]').tooltip();
}

// Initialize smooth scrolling for anchor links
function initSmoothScrolling() {
    $('a[href*="#"]:not([href="#"])').on('click', function () {
        if (location.pathname.replace(/^\//, '') === this.pathname.replace(/^\//, '') &&
            location.hostname === this.hostname) {
            var target = $(this.hash);
            target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top - 70 // Adjust for fixed header
                }, 800);
                return false;
            }
        }
    });
}

// Initialize toast notifications system
function initToastNotifications() {
    // Create toast container if not exists
    if ($('.toast-container').length === 0) {
        $('body').append('<div class="toast-container"></div>');
    }

    // Global function to show toast notifications
    window.showToast = function (message, type = 'info', autohide = true, delay = 5000) {
        // Create a unique ID for the toast
        var toastId = 'toast-' + Date.now();

        // Create toast HTML
        var toastHtml = `
            <div class="toast" id="${toastId}" role="alert" aria-live="assertive" aria-atomic="true" data-bs-autohide="${autohide}" data-bs-delay="${delay}">
                <div class="toast-header bg-${type} text-white">
                    <i class="fas fa-info-circle me-2"></i>
                    <strong class="me-auto">Notification</strong>
                    <small>Just now</small>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body">
                    ${message}
                </div>
            </div>
        `;

        // Append toast to container
        $('.toast-container').append(toastHtml);

        // Initialize and show the toast
        var toastElement = document.getElementById(toastId);
        var toast = new bootstrap.Toast(toastElement);
        toast.show();

        // Remove the toast element after it's hidden
        $(toastElement).on('hidden.bs.toast', function () {
            $(this).remove();
        });
    };

    // Listen for flash messages in the DOM and convert them to toasts
    $('.flash-message').each(function () {
        var message = $(this).text();
        var type = $(this).data('type') || 'info';
        showToast(message, type);
        $(this).remove();
    });
}

// Initialize dark mode toggle functionality
function initDarkModeToggle() {
    // Check for saved theme preference or use system preference
    const savedTheme = localStorage.getItem('theme');
    const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;

    if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
        document.documentElement.setAttribute('data-theme', 'dark');
        $('#darkModeToggle').prop('checked', true);
    }

    // Toggle dark mode when checkbox changes
    $('#darkModeToggle').on('change', function () {
        if ($(this).is(':checked')) {
            document.documentElement.setAttribute('data-theme', 'dark');
            localStorage.setItem('theme', 'dark');
        } else {
            document.documentElement.setAttribute('data-theme', 'light');
            localStorage.setItem('theme', 'light');
        }
    });

    // Listen for system theme changes
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
        if (!localStorage.getItem('theme')) {
            document.documentElement.setAttribute('data-theme', e.matches ? 'dark' : 'light');
            $('#darkModeToggle').prop('checked', e.matches);
        }
    });
}

// Enhance accessibility across the site
function enhanceAccessibility() {
    // Add skip to main content link
    if ($('.skip-link').length === 0) {
        $('body').prepend('<a href="#main" class="skip-link">Skip to main content</a>');
    }

    // Add appropriate ARIA roles and attributes
    $('nav').attr('role', 'navigation');
    $('main').attr('role', 'main').attr('id', 'main');
    $('footer').attr('role', 'contentinfo');

    // Add alt text to images that don't have it
    $('img:not([alt])').attr('alt', '');

    // Make sure all form fields have associated labels
    $('input, select, textarea').each(function () {
        if (!$(this).attr('id')) {
            $(this).attr('id', 'field-' + Math.random().toString(36).substring(2, 9));
        }

        if ($(this).closest('.form-group').find('label[for="' + $(this).attr('id') + '"]').length === 0) {
            var placeholderText = $(this).attr('placeholder') || 'Field';
            $(this).before('<label for="' + $(this).attr('id') + '" class="form-label">' + placeholderText + '</label>');
        }
    });
}

// Enhance form validation feedback
function enhanceFormValidation() {
    // Add custom validation styles and messages
    $('form.needs-validation').on('submit', function (event) {
        if (!this.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();

            // Find the first invalid field and focus it
            $(this).find(':invalid').first().focus();

            // Show custom validation messages
            $(this).find(':invalid').each(function () {
                var errorMessage = $(this).data('error-message') || 'Please check this field.';

                // Remove existing error messages
                $(this).siblings('.invalid-feedback').remove();

                // Add new error message
                $(this).after('<div class="invalid-feedback">' + errorMessage + '</div>');
            });

            // Show toast notification
            if (window.showToast) {
                window.showToast('Please correct the errors in the form.', 'danger');
            }
        }

        $(this).addClass('was-validated');
    });

    // Live validation on input
    $('form.needs-validation input, form.needs-validation select, form.needs-validation textarea').on('input change', function () {
        // Check validity of this field
        if (this.checkValidity()) {
            $(this).removeClass('is-invalid').addClass('is-valid');
        } else {
            $(this).removeClass('is-valid').addClass('is-invalid');
        }
    });
}

// Handle AJAX forms with better user feedback
function handleAjaxForms() {
    $('form.ajax-form').on('submit', function (event) {
        event.preventDefault();

        var $form = $(this);
        var url = $form.attr('action');
        var method = $form.attr('method') || 'post';
        var formData = $form.serialize();

        // Show loading state
        var $submitButton = $form.find('[type="submit"]');
        var originalButtonText = $submitButton.html();
        $submitButton.prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Processing...');

        // Add overlay to form
        var $formOverlay = $('<div class="form-overlay"><div class="spinner"></div></div>');
        $form.append($formOverlay);

        // Send AJAX request
        $.ajax({
            url: url,
            type: method,
            data: formData,
            success: function (response) {
                // Handle success
                if (window.showToast) {
                    window.showToast(response.message || 'Operation completed successfully.', 'success');
                }

                // Perform custom success action if defined
                if ($form.data('success-action') === 'redirect' && response.redirectUrl) {
                    window.location.href = response.redirectUrl;
                } else if ($form.data('success-action') === 'reset') {
                    $form.trigger('reset');
                } else if ($form.data('success-action') === 'reload') {
                    window.location.reload();
                }

                // Trigger custom success event
                $form.trigger('ajaxFormSuccess', [response]);
            },
            error: function (xhr) {
                // Handle error
                var errorMessage = 'An error occurred. Please try again.';

                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                }

                if (window.showToast) {
                    window.showToast(errorMessage, 'danger');
                }

                // Trigger custom error event
                $form.trigger('ajaxFormError', [xhr]);
            },
            complete: function () {
                // Restore button state
                $submitButton.prop('disabled', false).html(originalButtonText);

                // Remove overlay
                $formOverlay.remove();

                // Trigger custom complete event
                $form.trigger('ajaxFormComplete');
            }
        });
    });
}

// Highlight active navigation item
function highlightActiveNavigation() {
    var currentPath = window.location.pathname;

    // Highlight main nav
    $('.navbar-nav .nav-link').each(function () {
        var href = $(this).attr('href');

        if (href === currentPath || currentPath.startsWith(href) && href !== '/') {
            $(this).addClass('active');
            $(this).closest('.nav-item.dropdown').find('.nav-link').addClass('active');
        }
    });

    // Highlight sidebar nav if exists
    $('.sidebar-nav .nav-link').each(function () {
        var href = $(this).attr('href');

        if (href === currentPath || currentPath.startsWith(href) && href !== '/') {
            $(this).addClass('active');
            $(this).parents('.collapse').addClass('show');
            $(this).parents('.nav-item').find('.nav-link[data-bs-toggle="collapse"]').removeClass('collapsed');
        }
    });
}

// Add responsive behaviors
function addResponsiveBehaviors() {
    // Collapse navbar on link click on mobile
    $('.navbar-nav .nav-link').on('click', function () {
        if ($('.navbar-toggler').is(':visible')) {
            $('.navbar-toggler').trigger('click');
        }
    });

    // Adjust padding on small screens
    function adjustPadding() {
        if (window.innerWidth < 576) {
            $('.container').css('padding', '0 10px');
        } else {
            $('.container').css('padding', '');
        }
    }

    // Run on load and resize
    adjustPadding();
    $(window).on('resize', adjustPadding);

    // Back to top button
    if ($('#backToTop').length === 0) {
        $('body').append('<button id="backToTop" class="btn btn-primary btn-sm rounded-circle position-fixed" style="bottom: 20px; right: 20px; display: none;"><i class="fas fa-arrow-up"></i></button>');

        // Show/hide based on scroll position
        $(window).on('scroll', function () {
            if ($(this).scrollTop() > 300) {
                $('#backToTop').fadeIn();
            } else {
                $('#backToTop').fadeOut();
            }
        });

        // Scroll to top on click
        $('#backToTop').on('click', function () {
            $('html, body').animate({ scrollTop: 0 }, 500);
        });
    }
}