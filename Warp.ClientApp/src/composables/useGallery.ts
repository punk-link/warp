import { ref, onBeforeUnmount } from 'vue'

export function useGallery() {
  const items = ref<Array<{ file: File; url: string }>>([])

  function addFiles(list: FileList | File[]) {
    const arr = Array.from(list as any as File[])
    for (const f of arr) {
      const url = URL.createObjectURL(f)
      items.value.push({ file: f, url })
    }
  }

  function remove(index: number) {
    const it = items.value.splice(index, 1)[0]
    if (it) URL.revokeObjectURL(it.url)
  }

  onBeforeUnmount(() => {
    for (const it of items.value) {
      try {
        URL.revokeObjectURL(it.url)
      } catch {}
    }
    items.value = []
  })

  return { items, addFiles, remove }
}

