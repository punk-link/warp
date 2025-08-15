<template>
  <div class="app">
    <!-- <nav class="nav">
      <RouterLink to="/">Index</RouterLink>
      <RouterLink to="/preview">Preview</RouterLink>
      <RouterLink to="/entry">Entry</RouterLink>
      <RouterLink to="/deleted">Deleted</RouterLink>
      <RouterLink to="/privacy">Privacy</RouterLink>
    </nav> -->
    <RouterView />
    <Footer />
  </div>
</template>

<script setup lang="ts">
import Footer from './components/Footer.vue';
</script>

<style scoped>
.app { padding: 1rem; }
.nav { display: flex; gap: 1rem; margin-bottom: 1rem; }
</style>
