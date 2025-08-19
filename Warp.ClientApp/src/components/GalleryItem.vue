<template>
  <div v-if="src" :src="src" :alt="name || 'image'" id="image-container-{{ id }}" class="image-container overflow-visible animate-catchy-fade-in" data-image-id="{{ id }}">
    <img :src="src" :alt="name || 'image'" />
    <input v-if="editable" type="hidden" name="ImageIds" value="{{ id }}">
    <div v-if="editable" class="absolute bottom-0 w-full flex justify-center" style="transform: translateY(50%);">
      <a href="javascript:void(0);" class="delete-image-button btn btn-round" title="Delete">
        <i class="icofont-bin text-xl"></i>
      </a>
    </div>
  </div>
  <div v-else id="empty-image-container" class="image-container flex items-center justify-center border-primary border-2">
    <i class="icofont-plus text-gray-300 text-4xl"></i>
  </div>
</template>

<script setup lang="ts">
interface Props {
  id: string;
  src?: string;
  name?: string;
  editable?: boolean;
}

defineProps<Props>();

defineEmits<{ (e: 'remove'): void }>();
</script>
