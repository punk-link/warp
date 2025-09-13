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
    }
    ,
    advancedEditor: {
      imagesHint: 'Drag and drop or browse for images'
    },
    logo: {
      title: 'Warplyn', // do not translate
      beta: 'beta' // do not translate
    },
    footer: {
      taglineStrong: 'snap, share and say goodbye',
      tagline: "share the moments instantly, knowing they'll disappear in no time",
      links: {
        index: 'Index',
        privacy: 'Privacy',
        sources: 'Sources'
      },
      copyright: 'all rights reserved'
    },
    countdown: {
      label: 'the entry expires in'
    }
  }
} as const;
