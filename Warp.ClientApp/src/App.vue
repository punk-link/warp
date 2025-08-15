<template>
  <div class="app">
    <h1>Warplyn</h1>
    <p>Mounted under <code>/app</code>. Router-ready.</p>
    <nav class="nav">
      <RouterLink to="/">Home</RouterLink>
      <RouterLink to="/about">About</RouterLink>
    </nav>
    <RouterView />
  </div>
</template>

<script setup lang="ts">
</script>

<style scoped>
.app { padding: 1rem; }
.nav { display: flex; gap: 1rem; margin-bottom: 1rem; }
</style>
