<template>
  <div class="advanced-editor space-y-4">
    <div class="w-full p-2 md:w-1/2 mb-5 mx-auto">
      <div class="flex flex-col">
        <div class="mb-15">
          <img :src="baseUrl + 'img/text_icon.svg'" alt="Text content icon" class="w-6 h-6 mb-2" />
          <slot name="text"></slot>
        </div>
        <div class="flex flex-col mb-5">
          <img :src="baseUrl + 'img/image_icon.svg'" alt="Image gallery icon" class="w-6 h-6 mb-2" />
          <label class="form-label mb-3">{{ t('components.advancedEditor.imagesHint') }}</label>
          <slot name="gallery"></slot>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
const baseUrl = import.meta.env.BASE_URL;
const { t } = useI18n()
</script>
