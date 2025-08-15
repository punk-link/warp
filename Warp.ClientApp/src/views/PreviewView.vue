<template>
    <!-- moved from pages/Preview.vue -->
    <div class="min-h-screen">
        <div class="flex flex-col sm:flex-row items-baseline px-3">
            <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
                <!-- Logo -->
            </div>
            <div class="service-message-container w-full sm:w-1/2 flex justify-end md:justify-center">
                <span id="entry-created-message" class="text-secondary font-semibold text-base">
                    entry successfully created
                </span>
                <span id="link-copied-message" class="text-secondary font-semibold text-base hidden">
                    the link was copied successfully
                </span>
            </div>
        </div>

        <section class="px-3 my-5">
            <div class="flex flex-col items-center justify-around min-h-[75vh]">
                <!-- Countdown -->
                <div class="w-full md:w-1/2 flex justify-end mb-5">
                    <div class="text-right">
                        <div class="countdown text-gray-700 text-3xl font-sans-serif font-semibold">&nbsp;</div>
                        <div class="text-gray-400 text-xs">
                            the entry expires in
                        </div>
                    </div>
                </div>

                <!-- Content -->
                <article class="w-full md:w-1/2 bg-yellow-50 p-3 rounded-sm mb-10">
                    <div class="relative">
                        <div class="absolute -top-6 right-3 z-10">
                            <button id="edit-entry-button" class="btn btn-round btn-outline-primary hidden" title="Edit">
                                <i class="icofont-pencil-alt-2 text-xl"></i>
                            </button>
                        </div>
                        <div class="gallery pt-5 hidden"></div>
                        <div class="text-content font-sans-serif text-base pt-5"></div>
                    </div>
                </article>

                <!-- Action Buttons -->
                <div class="flex justify-between items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
                    <div class="bg-white rounded-sm">
                        <button id="delete-button" class="btn btn-outline-gray" title="Delete">
                            <i class="icofont-bin text-xl"></i>
                        </button>
                    </div>
                    <div class="bg-white rounded-sm">
                        <button id="edit-button" class="btn btn-outline-gray hidden" title="Clone and edit">
                            <i class="icofont-loop text-xl"></i>
                        </button>
                    </div>
                    <div class="bg-white rounded-sm">
                        <button id="copy-link-button" class="btn btn-primary" title="Copy link">
                            <i class="icofont-link text-white/50"></i>
                            Copy link
                        </button>
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>
