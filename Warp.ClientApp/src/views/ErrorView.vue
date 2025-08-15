<template>
  <!-- moved from pages/Error.vue -->
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
        <div class="w-full md:w-1/2 mb-5">
          <!-- Error Content -->
          <div class="flex flex-col">
            <div class="text-white text-9xl font-sans-serif">500</div>

            <div class="mb-8">
              <p class="mb-10 text-white text-xl">An unexpected error occurred.</p>

              <div class="text-base text-gray-700 text-sm relative">
                <span class="font-semibold">Request ID:</span>
                <code id="request-id-code"
                      class="bg-gray-100 p-1 rounded border border-transparent hover:bg-gray-50 hover:border-secondary cursor-pointer transition-all duration-200"
                      title="Click to copy">
                  <span class="text-gray-400 select-none">REQ-</span><span>12345678</span><span class="text-gray-500">...</span>
                  <span id="request-id-value" class="hidden">12345678-1234-1234-1234-1234567890ab</span>
                  <i class="icofont-copy ml-1 text-gray-400"></i>
                </code>
                <span id="copy-tooltip"
                      class="hidden absolute -top-8 left-1/2 transform -translate-x-1/2 text-xs font-semibold text-white bg-secondary px-2 py-1 rounded shadow-md">
                  Copied!
                </span>
                <p class="text-xs text-gray-500 mt-1">
                  Include this ID when reporting issues
                </p>
              </div>
            </div>

            <!-- Optional: errors list container (static) -->
            <div class="mt-4 hidden">
              <h3 class="text-xl text-gray-700 font-semibold">Errors:</h3>
              <div class="mb-2 text-base text-gray-700">
                <span class="font-semibold">E000.</span> Sample error message
              </div>
            </div>
          </div>
        </div>

        <!-- Action Button -->
        <div class="flex justify-center items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
          <div>
            <a href="/" class="btn btn-primary">
              <i class="icofont-simple-left text-white/50"></i>
              Go to main
            </a>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>
