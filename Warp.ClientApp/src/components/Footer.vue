<template>
    <footer class="my-15">
        <!-- <div id="roaming-image" class="hidden fixed -right-[35vh] -bottom-[25vh] w-[75vh] h-[75vh] max-h-[75vh] overflow-hidden -z-10">
            <img
                src="/img/warp.svg"
                alt="background illustration"
                class="opacity-50 w-full h-full object-contain"
                style="filter: invert(97%) sepia(3%) saturate(2195%) hue-rotate(344deg) brightness(106%) contrast(102%);"
            />
        </div> -->
        <h1 class="logo-style text-gray-400 text-4xl mb-4">
            {{ t('components.logo.title') }}
        </h1>
        <p class="text-3xl text-gray-600 font-light mt-4">
            <strong>
                {{ t('components.footer.taglineStrong') }}
            </strong>
            <br />
            {{ t('components.footer.tagline') }}
        </p>
        <nav class="text-gray-400 text-sm mt-3">
            <router-link to="/" class="hover:underline">{{ t('components.footer.links.index') }}</router-link>
            <span class="mx-2">|</span>
            <router-link to="/privacy" class="hover:underline">{{ t('components.footer.links.privacy') }}</router-link>
            <span class="mx-2">|</span>
            <router-link to="/data-request" class="hover:underline">{{ t('components.footer.links.dataRequest') }}</router-link>
            <span class="mx-2">|</span>
            <router-link to="/feedback" class="hover:underline">{{ t('components.footer.links.feedback') }}</router-link>
            <span class="mx-2">|</span>
            <a href="https://github.com/punk-link/warp" target="_blank" rel="noopener" class="hover:underline">{{ t('components.footer.links.sources') }}</a>
        </nav>
        <p class="text-gray-300 text-sm mt-3">
            &copy; warplyn.com {{ yearDisplay }} | {{ t('components.footer.copyright') }}
        </p>
    </footer>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'


const { t } = useI18n()


const startYear = 2024;
const currentYear = new Date().getFullYear();
const yearDisplay = startYear === currentYear 
    ? `${startYear}` 
    : `${startYear}\u2013${currentYear}`;
</script>
