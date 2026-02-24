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

    dataRequest: {
        title: 'Solicitud de datos',
        description: 'Tienes derecho a solicitar una copia de tus datos personales que procesamos. Para solicitar tus datos, ponte en contacto con nosotros por correo electrónico.',
        instructions: 'Envía tu solicitud de datos a:',
        sendEmailButton: 'Enviar solicitud',
        note: 'Por favor, incluye los detalles de tu cuenta o cualquier información relevante que pueda ayudarnos a identificar tus datos en tu correo electrónico.',
        goHome: 'Ir al inicio'
    },

    feedback: {
        title: 'Enviar comentarios',
        description: '¡Agradecemos tus comentarios! Envíanos tus sugerencias, problemas o comentarios generales por correo electrónico.',
        instructions: 'Apreciamos todas las ideas de producto, quejas y reportes de errores. Simplemente haz clic en el botón a continuación para abrir tu cliente de correo:',
        sendEmailButton: 'Enviar correo de comentarios',
        note: 'Tus comentarios nos ayudan a mejorar el servicio. ¡Gracias por tomarte el tiempo de compartir tus pensamientos!',
        goHome: 'Ir al inicio'
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
            beta: 'beta 4' // do not translate
        },
        footer: {
            taglineStrong: 'captura, comparte y di adiós',
            tagline: 'comparte los momentos al instante, sabiendo que desaparecerán en poco tiempo',
            links: {
                index: 'Índice',
                privacy: 'Privacidad',
                dataRequest: 'Solicitud de datos',
                feedback: 'Comentarios',
                sources: 'Fuentes'
            },
            copyright: 'todos los derechos reservados'
        },
        countdown: {
            label: 'la entrada expira en'
        },
        notifications: {
            close: 'Cerrar',
            ariaClose: 'Cerrar notificación',
            copyDetails: 'Copiar detalles',
            copyDiagnostics: 'Copiar identificadores de diagnóstico'
        },
        contentSizeIndicator: {
            exceeded: 'Límite de tamaño de contenido excedido',
            approaching: 'Aproximándose al límite de tamaño',
            large: 'El tamaño del contenido es grande'
        },
        richTextEditor: {
            toolbar: {
                bold: 'Negrita',
                italic: 'Cursiva',
                underline: 'Subrayado',
                strikethrough: 'Tachado',
                heading1: 'Encabezado 1',
                heading2: 'Encabezado 2',
                heading3: 'Encabezado 3',
                bulletList: 'Lista de viñetas',
                orderedList: 'Lista ordenada',
                blockquote: 'Cita',
                codeBlock: 'Bloque de código',
                link: 'Enlace'
            },
            linkPrompt: 'URL'
        }
    },

    api: {
        error: {
            network: 'Error de red. Verifica tu conexión.',
            generic: 'Algo salió mal.',
            _400: 'Solicitud incorrecta',
            _401: 'Sesión expirada. Inicia sesión nuevamente.',
            _403: 'Acceso denegado',
            _404: 'No encontrado',
            _409: 'Conflicto',
            _422: 'Error de validación',
            _429: 'Demasiadas solicitudes. Inténtalo más tarde.'
        }
    }
} as const;
