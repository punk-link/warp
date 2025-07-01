import { core } from '/js/core/initialize.js';
import { IndexPageController } from './index-page-controller.js';
import { elements } from './elements.js';

// Initialize core functionality
core.initialize();

// Global page controller instance
let pageController = null;

/**
 * Initializes all index page events and functionality
 */
export const addIndexEvents = async () => {
    try {
        pageController = new IndexPageController(elements);
        await pageController.initialize();
        
        // Set up cleanup on page unload
        window.addEventListener('beforeunload', () => {
            pageController?.cleanup();
        });
        
    } catch (error) {
        console.error('Failed to initialize index page:', error);
    }
};

/**
 * Sets up textareas with proper sizing based on edit mode
 */
export const setupTextareas = (currentEditMode) => {
    try {
        pageController?.setupTextareas(currentEditMode);
    } catch (error) {
        console.error('Failed to setup textareas:', error);
    }
};

// Make functions available globally for Razor Pages
window.addIndexEvents = addIndexEvents;
window.setupTextareas = setupTextareas;