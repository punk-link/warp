// Spanish locale messages
export default {
  app: {
    title: 'Warplyn',
    actions: {
      copy: 'Copiar',
      cancel: 'Cancelar',
      delete: 'Eliminar',
      close: 'Cerrar',
      save: 'Guardar',
      edit: 'Editar',
      report: 'Reportar'
    }
  },

  language: 'Idioma',

  home: {
    mode: {
      text: 'Texto',
      advanced: 'Avanzado'
    },
    editor: {
      textLabel: 'Texto',
      textPlaceholder: 'Escribe o pega tu texto aquí',
      preview: 'Vista previa',
      expiresIn: 'Expira en',
      previewButton: 'Vista previa'
    },
    gallery: {
      emptyItemAria: 'Añadir imágenes',
      removeImage: 'Eliminar imagen'
    }
  },

  entry: {
    viewCountLabel: '{count} visitas',
    loading: 'cargando...',
    reported: 'reportado',
    linkCopied: 'enlace copiado',
    actions: {
      report: 'Reportar',
      close: 'Cerrar',
      copyLink: 'Copiar enlace'
    },
    reportModal: {
      title: 'Reportar contenido',
      description: 'Estás a punto de reportar este contenido. Usa esta función solo en caso de contenido inapropiado. Esta acción restringe el acceso al contenido para todos los usuarios. ¿Estás seguro?',
      confirm: 'Reportar',
      cancelling: 'Cancelar',
      reporting: 'Reportando...'
    }
  },

  preview: {
    loading: 'cargando...',
    saving: 'guardando...',
    failedToLoad: 'no se pudo cargar la entrada',
    actions: {
      save: 'Guardar',
      delete: 'Eliminar',
      cloneEdit: 'Clonar y editar',
      copyLink: 'Copiar enlace'
    }
  },

  deleted: {
    message: 'la entrada fue eliminada',
    create: 'Crear'
  },

  error: {
    defaultNotFound: 'La página que buscas no existe.',
    defaultUnexpected: 'Ocurrió un error inesperado.',
    requestIdLabel: 'ID de solicitud:',
    goHome: 'Ir al inicio',
    errorsTitle: 'Errores:'
  },

  privacy: {
    loading: 'Cargando…',
    notAvailableTitle: 'Política de privacidad',
    notAvailableContent: 'Contenido no disponible.'
  },

  components: {
    expirationSelect: {
      label: 'Expira en',
      options: {
        FiveMinutes: '5 minutos',
        ThirtyMinutes: '30 minutos',
        OneHour: '1 hora',
        EightHours: '8 horas',
        OneDay: '1 día'
      }
    },
    buttons: {
      primary: 'Aceptar',
      cancel: 'Cancelar',
      pending: 'Pendiente…'
    },
    advancedEditor: {
      imagesHint: 'Arrastra y suelta o busca imágenes'
    },
    logo: {
      title: 'Warplyn', // do not translate
      beta: 'beta' // do not translate
    },
    footer: {
      taglineStrong: 'captura, comparte y di adiós',
      tagline: 'comparte los momentos al instante, sabiendo que desaparecerán en poco tiempo',
      links: {
        index: 'Índice',
        privacy: 'Privacidad',
        sources: 'Fuentes'
      },
      copyright: 'todos los derechos reservados'
    },
    countdown: {
      label: 'la entrada expira en'
    }
  }
} as const;
