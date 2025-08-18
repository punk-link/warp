<template>
  <div class="relative group">
    <img v-if="src" :src="src" :alt="name || 'image'" class="rounded object-cover max-h-40" />
    <div v-else class="p-4 bg-gray-800 rounded text-gray-300 text-sm">{{ name }}</div>
    <button
      type="button"
      class="absolute top-1 right-1 bg-black/60 hover:bg-black/80 text-white text-xs rounded px-2 py-1"
      @click="$emit('remove')"
      aria-label="Remove image"
    >
      ×
    </button>
  </div>
</template>

<script setup lang="ts">
interface Props {
  src?: string;
  name?: string;
}

defineProps<Props>();

defineEmits<{ (e: 'remove'): void }>();
</script>
