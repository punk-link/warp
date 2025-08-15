<template>
  <!-- moved from pages/Index.vue -->
  <div class="min-h-screen">
    <div class="flex flex-col sm:flex-row items-baseline px-3">
      <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
  <Logo />
      </div>
      <div class="w-full sm:w-1/2 flex justify-end md:justify-center">
        <button id="simple-mode-nav-button" class="nav-button" disabled>
          Text mode
        </button>
        <button id="advanced-mode-nav-button" class="nav-button" disabled>
          Advanced mode
        </button>
      </div>
    </div>

    <section class="px-3 my-5">
      <form id="entry-form" method="post">
        <div class="flex flex-col items-center justify-around min-h-[75vh]">
          <!-- Simple Mode Container -->
          <div id="simple-mode-container" class="w-full">
            <div class="w-full md:w-1/2 mb-5 mx-auto">
              <div class="flex flex-col">
                <textarea id="simple-text-content" class="form-textarea order-2 bg-transparent" placeholder=" "></textarea>
                <label class="form-label floating-label order-1" for="simple-text-content">Text</label>
              </div>
            </div>
          </div>

          <!-- Advanced Mode Container -->
          <div id="advanced-mode-container" class="w-full">
            <div class="drop-area w-full p-2 md:w-1/2 mb-5 mx-auto">
              <div class="flex flex-col">
                <div class="flex flex-col mb-15">
                  <img src="/img/text_icon.svg" class="inline-block w-6 h-6 mb-2" alt="" aria-hidden="true" />
                  <label class="form-label" for="advanced-text-content">Text</label>
                  <textarea id="advanced-text-content" class="form-textarea bg-transparent" placeholder=" "></textarea>
                </div>
                <div class="flex flex-col mb-5">
                  <img src="/img/image_icon.svg" class="inline-block w-6 h-6 mb-2" alt="" aria-hidden="true" />
                  <label class="form-label mb-3">Image</label>
                  <div class="gallery"></div>
                  <input type="file" id="file" class="file-input hidden" multiple="multiple" accept="image/*">
                </div>
              </div>
            </div>
          </div>

          <!-- Shared Form Controls -->
          <div class="flex justify-between items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
            <div class="bg-white rounded-sm">
              <label class="form-label" for="expiration-selector">Expiration period</label>
              <div class="flex items-baseline">
                <i class="icofont-close text-primary text-base mr-2"></i>
                <select id="expiration-selector" class="form-select"></select>
              </div>
            </div>
            <div class="bg-white rounded-sm">
              <button id="create-button" class="btn btn-primary" type="submit" disabled>
                Create
              </button>
            </div>
          </div>

          <input type="hidden" name="id" />
          <input id="edit-mode-state" type="hidden" />
        </div>
      </form>
    </section>
  </div>
</template>

<script setup lang="ts">
import Logo from '../components/Logo.vue'
</script>
