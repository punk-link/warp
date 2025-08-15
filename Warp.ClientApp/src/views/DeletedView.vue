<template>
  <!-- moved from pages/Deleted.vue -->
  <div class="min-h-screen">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row items-baseline px-3">
      <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
        <!-- Logo -->
      </div>
    </div>

    <!-- Main Content -->
    <section class="px-3 my-5">
      <div class="flex flex-col items-center justify-around min-h-[75vh]">
        <div class="w-full md:w-1/2 flex flex-col justify-center items-center mb-5">
          <i class="icofont-bin text-gray-300 text-7xl mb-5"></i>
          <span class="text-gray-400 text-base font-semibold">
            the entry was deleted
          </span>
        </div>

        <!-- Action Button -->
        <div class="flex justify-center items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
          <div class="bg-white rounded-sm">
            <button id="create-button" class="btn btn-primary">
              <i class="icofont-plus text-white/50"></i>
              Create
            </button>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>
