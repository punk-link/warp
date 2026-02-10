// English locale messages (authoritative base for type inference)
export default {
    app: {
        title: 'Warplyn',
        actions: {
            copy: 'Copy',
            cancel: 'Cancel',
            delete: 'Delete',
            close: 'Close',
            save: 'Save',
            edit: 'Edit',
            report: 'Report'
        }
    },

    language: 'Language',

    home: {
        mode: {
            text: 'Text',
            advanced: 'Advanced'
        },
        editor: {
            textLabel: 'Text',
            textPlaceholder: 'Type or paste your text here',
            preview: 'Preview',
            expiresIn: 'Expires in',
            previewButton: 'Preview'
        },
        gallery: {
            emptyItemAria: 'Add images',
            removeImage: 'Remove image'
        }
    },

    entry: {
        viewCountLabel: '{count} views',
        loading: 'loading...',
        reported: 'reported',
        linkCopied: 'link copied',
        actions: {
            report: 'Report',
            close: 'Close',
            copyLink: 'Copy Link'
        },
        reportModal: {
            title: 'Report content',
            description: 'You are about to report this content. Use the feature in case of inappropriate content only. This action restricts access to the content for all viewers. Are you sure?',
            confirm: 'Report',
            cancelling: 'Cancel',
            reporting: 'Reporting...'
        }
    },

    preview: {
        loading: 'loading...',
        saving: 'saving...',
        failedToLoad: 'failed to load entry',
        actions: {
            save: 'Save',
            delete: 'Delete',
            cloneEdit: 'Clone & Edit',
            copyLink: 'Copy Link'
        }
    },

    deleted: {
        message: 'the entry was deleted',
        create: 'Create'
    },

    error: {
        defaultNotFound: 'The page you are looking for does not exist.',
        defaultUnexpected: 'An unexpected error occurred.',
        requestIdLabel: 'Request ID:',
        goHome: 'Go to main',
        errorsTitle: 'Errors:'
    },

    privacy: {
        loading: 'Loading…',
        notAvailableTitle: 'Privacy Policy',
        notAvailableContent: 'Content not available.'
    },

    dataRequest: {
        title: 'Data Request',
        description: 'You have the right to request a copy of your personal data that we process. To request your data, please contact us via email.',
        instructions: 'Send your data request to:',
        note: 'Please include your account details or any relevant information that can help us identify your data in your email.',
        goHome: 'Go to main'
    },

    components: {
        expirationSelect: {
            label: 'Expires in',
            options: {
                FiveMinutes: '5 minutes',
                ThirtyMinutes: '30 minutes',
                OneHour: '1 hour',
                EightHours: '8 hours',
                OneDay: '1 day'
            }
        },
        buttons: {
            primary: 'OK',
            cancel: 'Cancel',
            pending: 'Pending…'
        },
        advancedEditor: {
            imagesHint: 'Drag and drop or browse for images'
        },
        logo: {
            title: 'Warplyn', // do not translate
            beta: 'beta 4' // do not translate
        },
        footer: {
            taglineStrong: 'snap, share and say goodbye',
            tagline: "share the moments instantly, knowing they'll disappear in no time",
            links: {
                index: 'Index',
                privacy: 'Privacy',
                dataRequest: 'Data Request',
                sources: 'Sources'
            },
            copyright: 'all rights reserved'
        },
        countdown: {
            label: 'the entry expires in'
        },
        notifications: {
            close: 'Close',
            ariaClose: 'Close notification',
            copyDetails: 'Copy details',
            copyDiagnostics: 'Copy diagnostic identifiers'
        }
    },

    api: {
        error: {
            network: 'Network error. Check your connection.',
            generic: 'Something went wrong.',
            _400: 'Bad request',
            _401: 'Session expired. Please sign in again.',
            _403: 'Access denied',
            _404: 'Not found',
            _409: 'Conflict',
            _422: 'Validation error',
            _429: 'Too many requests. Try again later.'
        }
    }
} as const;
