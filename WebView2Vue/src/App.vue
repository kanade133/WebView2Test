<template>
  <Home />
</template>

<script setup>
import { useDark, useToggle } from '@vueuse/core'
import Home from './views/Home.vue'

const isDark = useDark()
if (!isDark.value) {
  const toggleDark = useToggle(isDark)
  toggleDark()
}
</script>

<style scoped></style>
