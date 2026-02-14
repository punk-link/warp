<template>
    <div class="rich-text-editor flex flex-col">
        <div v-if="editor" class="editor-toolbar flex flex-wrap gap-1 mb-2 pb-2 border-b border-gray-200">
            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('bold') }"
                @click="editor.chain().focus().toggleBold().run()" :disabled="!editable"
                title="Bold" aria-label="Bold">
                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M6 4h8a4 4 0 0 1 4 4 4 4 0 0 1-4 4H6z" />
                    <path d="M6 12h9a4 4 0 0 1 4 4 4 4 0 0 1-4 4H6z" />
                </svg>
            </button>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('italic') }"
                @click="editor.chain().focus().toggleItalic().run()" :disabled="!editable"
                title="Italic" aria-label="Italic">
                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="19" y1="4" x2="10" y2="4" />
                    <line x1="14" y1="20" x2="5" y2="20" />
                    <line x1="15" y1="4" x2="9" y2="20" />
                </svg>
            </button>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('underline') }"
                @click="editor.chain().focus().toggleUnderline().run()" :disabled="!editable"
                title="Underline" aria-label="Underline">
                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M6 3v7a6 6 0 0 0 6 6 6 6 0 0 0 6-6V3" />
                    <line x1="4" y1="21" x2="20" y2="21" />
                </svg>
            </button>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('strike') }"
                @click="editor.chain().focus().toggleStrike().run()" :disabled="!editable"
                title="Strikethrough" aria-label="Strikethrough">
                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M16 4H9a3 3 0 0 0-2.83 4" />
                    <path d="M14 12a4 4 0 0 1 0 8H6" />
                    <line x1="4" y1="12" x2="20" y2="12" />
                </svg>
            </button>

            <div class="toolbar-divider"></div>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('heading', { level: 1 }) }"
                @click="editor.chain().focus().toggleHeading({ level: 1 }).run()" :disabled="!editable"
                title="Heading 1" aria-label="Heading 1">
                <span class="font-semibold">H1</span>
            </button>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('heading', { level: 2 }) }"
                @click="editor.chain().focus().toggleHeading({ level: 2 }).run()" :disabled="!editable"
                title="Heading 2" aria-label="Heading 2">
                <span class="font-semibold">H2</span>
            </button>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('heading', { level: 3 }) }"
                @click="editor.chain().focus().toggleHeading({ level: 3 }).run()" :disabled="!editable"
                title="Heading 3" aria-label="Heading 3">
                <span class="font-semibold">H3</span>
            </button>

            <div class="toolbar-divider"></div>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('bulletList') }"
                @click="editor.chain().focus().toggleBulletList().run()" :disabled="!editable"
                title="Bullet List" aria-label="Bullet List">
                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="8" y1="6" x2="21" y2="6" />
                    <line x1="8" y1="12" x2="21" y2="12" />
                    <line x1="8" y1="18" x2="21" y2="18" />
                    <circle cx="3" cy="6" r="1" fill="currentColor" />
                    <circle cx="3" cy="12" r="1" fill="currentColor" />
                    <circle cx="3" cy="18" r="1" fill="currentColor" />
                </svg>
            </button>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('orderedList') }"
                @click="editor.chain().focus().toggleOrderedList().run()" :disabled="!editable"
                title="Ordered List" aria-label="Ordered List">
                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="10" y1="6" x2="21" y2="6" />
                    <line x1="10" y1="12" x2="21" y2="12" />
                    <line x1="10" y1="18" x2="21" y2="18" />
                    <path d="M4 6h1v4" />
                    <path d="M4 10h2" />
                    <path d="M6 18H4c0-1 2-2 2-3s-1-1.5-2-1" />
                </svg>
            </button>

            <div class="toolbar-divider"></div>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('blockquote') }"
                @click="editor.chain().focus().toggleBlockquote().run()" :disabled="!editable"
                title="Blockquote" aria-label="Blockquote">
                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M3 21c3 0 7-1 7-8V5c0-1.25-.756-2.017-2-2H4c-1.25 0-2 .75-2 1.972V11c0 1.25.75 2 2 2 1 0 1 0 1 1v1c0 1-1 2-2 2s-1 .008-1 1.031V20c0 1 0 1 1 1z" />
                    <path d="M15 21c3 0 7-1 7-8V5c0-1.25-.757-2.017-2-2h-4c-1.25 0-2 .75-2 1.972V11c0 1.25.75 2 2 2h.75c0 2.25.25 4-2.75 4v3c0 1 0 1 1 1z" />
                </svg>
            </button>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('codeBlock') }"
                @click="editor.chain().focus().toggleCodeBlock().run()" :disabled="!editable"
                title="Code Block" aria-label="Code Block">
                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="16 18 22 12 16 6" />
                    <polyline points="8 6 2 12 8 18" />
                </svg>
            </button>

            <div class="toolbar-divider"></div>

            <button type="button" class="toolbar-btn" :class="{ 'is-active': editor.isActive('link') }"
                @click="toggleLink" :disabled="!editable"
                title="Link" aria-label="Link">
                <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71" />
                    <path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71" />
                </svg>
            </button>
        </div>

        <div class="editor-wrapper">
            <EditorContent :editor="editor" class="editor-content" />
        </div>
        <Transition
            enter-active-class="transition-all duration-200 ease-out"
            enter-from-class="opacity-0 -translate-y-1"
            enter-to-class="opacity-100 translate-y-0"
            leave-active-class="transition-all duration-150 ease-in"
            leave-from-class="opacity-100 translate-y-0"
            leave-to-class="opacity-0 -translate-y-1"
        >
            <div v-if="showSizeWarning" class="flex items-center justify-end gap-2 mt-1">
                <span class="text-xs text-gray-400">{{ sizeWarningText }}</span>
                <div class="w-2 h-2 rounded-full flex-shrink-0 transition-colors duration-300" :class="circleColor"></div>
            </div>
        </Transition>
    </div>
</template>

<script setup lang="ts">
import { watch, onBeforeUnmount, ref, computed } from 'vue'
import { useEditor, EditorContent } from '@tiptap/vue-3'
import type { JSONContent } from '@tiptap/core'
import StarterKit from '@tiptap/starter-kit'
import Link from '@tiptap/extension-link'
import Underline from '@tiptap/extension-underline'
import { useI18n } from 'vue-i18n'
import { getByteSize, getSizeIndicatorState, maxHtmlContentSize, maxContentDeltaSize } from '../composables/use-content-size-indicator'


interface Props {
    modelValue: string;
    editable?: boolean;
    placeholder?: string;
}


const { t } = useI18n()


const props = withDefaults(defineProps<Props>(), {
    modelValue: '',
    editable: true,
    placeholder: '',
})


const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
    (e: 'update:html', value: string): void;
    (e: 'update:textLength', value: number): void;
    (e: 'update:sizeWarning', htmlBytes: number, jsonBytes: number, isOverLimit: boolean): void;
}>()


const currentHtml = ref('')
const currentJson = ref('')


const htmlSizeBytes = computed(() => getByteSize(currentHtml.value))
const jsonSizeBytes = computed(() => getByteSize(currentJson.value))


const htmlState = computed(() => getSizeIndicatorState(htmlSizeBytes.value, maxHtmlContentSize))
const jsonState = computed(() => getSizeIndicatorState(jsonSizeBytes.value, maxContentDeltaSize))


const isOverLimit = computed(() => htmlSizeBytes.value > maxHtmlContentSize || jsonSizeBytes.value > maxContentDeltaSize)


const showSizeWarning = computed(() => htmlState.value.showWarning || jsonState.value.showWarning)
const circleColor = computed(() => {
    const htmlPercent = (htmlSizeBytes.value / maxHtmlContentSize) * 100
    const jsonPercent = (jsonSizeBytes.value / maxContentDeltaSize) * 100
    return htmlPercent >= jsonPercent ? htmlState.value.circleColor : jsonState.value.circleColor
})
const sizeWarningText = computed(() => {
    const htmlPercent = (htmlSizeBytes.value / maxHtmlContentSize) * 100
    const jsonPercent = (jsonSizeBytes.value / maxContentDeltaSize) * 100
    
    const dominantState = htmlPercent >= jsonPercent ? htmlState.value : jsonState.value
    const key = dominantState.warningKey
    
    if (!key)
        return null
    
    return t(`components.contentSizeIndicator.${key}`)
})

function parseContent(value: string): JSONContent | string {
    if (!value || value.trim() === '')
        return ''

    try {
        return JSON.parse(value) as JSONContent
    } catch {
        return value
    }
}


const editor = useEditor({
    content: parseContent(props.modelValue),
    editable: props.editable,
    extensions: [
        StarterKit.configure({
            heading: {
                levels: [1, 2, 3],
            },
        }),
        Link.configure({
            openOnClick: false,
            HTMLAttributes: {
                rel: 'noopener noreferrer nofollow',
                target: '_blank',
            },
        }),
        Underline,
    ],
    editorProps: {
        attributes: {
            class: 'tiptap-editor rich-text-content min-h-40 max-h-96 overflow-y-auto px-3 py-2 rounded-none focus:outline-none transition-all duration-200 border-0 border-b-2 border-primary',
            placeholder: props.placeholder,
        },
    },
    onUpdate: () => {
        if (!editor.value)
            return

        const json = editor.value.getJSON()
        const html = editor.value.getHTML()
        const textLength = editor.value.getText().length
        const jsonString = JSON.stringify(json)

        currentHtml.value = html
        currentJson.value = jsonString

        emit('update:modelValue', jsonString)
        emit('update:html', html)
        emit('update:textLength', textLength)
        emit('update:sizeWarning', htmlSizeBytes.value, jsonSizeBytes.value, isOverLimit.value)
    },
})


watch(() => props.modelValue, (newValue) => {
    if (!editor.value)
        return

    const currentJsonString = JSON.stringify(editor.value.getJSON())
    if (currentJsonString === newValue)
        return

    const content = parseContent(newValue)
    editor.value.commands.setContent(content, { emitUpdate: false })
})


watch(() => props.editable, (newValue) => {
    if (!editor.value)
        return

    editor.value.setEditable(newValue)
})


function toggleLink() {
    if (!editor.value)
        return

    const previousUrl = editor.value.getAttributes('link').href as string | undefined
    const url = window.prompt('URL', previousUrl)

    if (url === null)
        return

    if (url === '') {
        editor.value.chain().focus().extendMarkRange('link').unsetLink().run()
        return
    }

    editor.value.chain().focus().extendMarkRange('link').setLink({ href: url }).run()
}


onBeforeUnmount(() => {
    if (editor.value)
        editor.value.destroy()
})
</script>

<style scoped>
.editor-content :deep(.tiptap-editor p.is-editor-empty:first-child::before) {
    color: rgb(156 163 175);
    float: left;
    height: 0;
    pointer-events: none;
    content: attr(data-placeholder);
}
</style>
