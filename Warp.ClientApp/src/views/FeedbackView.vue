<template>
    <div class="min-h-screen">
        <div class="flex flex-col sm:flex-row items-baseline px-3">
            <div class="w-full sm:w-1/2 flex justify-start md:justify-center">
                <Logo />
            </div>
        </div>

        <section class="px-3 my-5">
            <div class="flex flex-col items-center justify-around min-h-[75vh]">
                <div class="w-full md:w-1/2 flex flex-col justify-center items-start mb-5 space-y-4">
                    <h1 class="text-gray-700 text-3xl font-semibold">
                        {{ t('feedback.title') }}
                    </h1>
                    <p class="text-gray-600 text-base">
                        {{ t('feedback.description') }}
                    </p>
                    <p class="text-gray-600 text-base">
                        {{ t('feedback.instructions') }}
                    </p>
                    <div class="flex items-center gap-2 flex-wrap">
                        <a :href="`mailto:${feedbackEmail}`" class="btn btn-primary no-underline">
                            {{ t('feedback.sendEmailButton') }}
                        </a>
                        <span class="text-gray-500 text-sm">{{ feedbackEmail }}</span>
                    </div>
                    <p class="text-gray-500 text-sm">
                        {{ t('feedback.note') }}
                    </p>
                </div>

                <div class="flex justify-center items-center w-full md:w-1/2 pb-3 sticky bottom-0 bg-transparent">
                    <div class="bg-white rounded-sm">
                        <Button variant="outline-primary" @click="goHome" :label="t('feedback.goHome')" icon-class="icofont-home" />
                    </div>
                </div>
            </div>
        </section>
    </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import Logo from '../components/Logo.vue'
import Button from '../components/Button.vue'
import { ViewNames } from '../router/enums/view-names'


const router = useRouter();
const { t } = useI18n()


const feedbackEmail = window.appConfig?.feedbackEmail;


function goHome() {
    router.push({ name: ViewNames.Home });
}
</script>
