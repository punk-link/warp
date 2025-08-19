<template>
  <div class="advanced-editor space-y-4">
    <div class="w-full p-2 md:w-1/2 mb-5 mx-auto">
      <div class="flex flex-col">
        <div class="mb-15">
          <i class="icofont-align-left text-secondary text-2xl mb-2"></i>
          <slot name="text"></slot>
        </div>
        <div class="flex flex-col mb-5">
          <i class="icofont-hill-sunny text-secondary text-2xl mb-2"></i>
          <label class="form-label mb-3">Drag and drop or browse for images</label>
          <slot name="gallery"></slot>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// Container component to compose text + gallery UI in advanced mode.
</script>
