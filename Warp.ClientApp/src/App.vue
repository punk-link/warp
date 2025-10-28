<template>
  <div :class="['app flex flex-col flex-1 min-h-screen w-full px-4 py-6 md:px-8 md:py-8', route.meta.pageBg]">
    <RouterView />
    <NotificationCenter />
    <Footer />
  </div>
</template>

<script setup lang="ts">
import Footer from './components/Footer.vue';
import NotificationCenter from './components/NotificationCenter.vue';
import { useRoute } from 'vue-router';

const route = useRoute();
</script>

