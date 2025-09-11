// English locale messages (authoritative base for type inference)
export default {
  app: {
    // Main product display name (avoid translating unless brand policy changes)
    title: 'Warp',
    // Short marketing tagline displayed under logo in the header
    tagline: 'Ephemeral sharing made simple',
    actions: {
      // Copies current entry URL or text content to clipboard
      copy: 'Copy',
      // Aborts the current form / modal without persisting changes
      cancel: 'Cancel',
      // Irreversibly removes an item (ask for confirmation externally)
      delete: 'Delete',
      // Closes a modal / panel (non-destructive)
      close: 'Close'
    },
    nav: {
      // Primary navigation links
      home: 'Home',
      about: 'About'
    }
  }
} as const;
