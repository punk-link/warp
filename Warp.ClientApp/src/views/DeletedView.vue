<template>
    <!-- moved from pages/Deleted.vue -->
    <div class="min-h-screen">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row items-baseline px-3">
            <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
                <Logo />
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
                        <Button variant="primary" @click="onCreate" label="Create" icon-class="icofont-plus text-white/50" />
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import Logo from '../components/Logo.vue'
import Button from '../components/Button.vue'


const router = useRouter();


function onCreate() {
    router.push({ name: 'Home' });
}
</script>
