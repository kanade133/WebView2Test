<template>
  <div ref="chartRef" class="chart-main"></div>
</template>

<script setup>
import { onMounted, onUnmounted, ref, reactive, computed, watch } from 'vue'
import ChartInstance from './chart'

const props = defineProps({
  data: { type: Array },
  name: { type: String },
})

const dateFormat = 'YYYY/MM/DD\nHH:mm:ss'

const chartRef = ref(null)
/** @type {ChartInstance} */
let chartInstance

watch(props.data, data => {
  if (chartInstance) {
    chartInstance.update({
      dataArray: props.data,
      serieName: props.name,
      dateFormat,
    })
  }
})

let resizeObserver = null
let isChartInitialized = false
onMounted(() => {
  const init = () => {
    chartInstance = new ChartInstance(chartRef.value)
    chartInstance.update({
      dataArray: props.data,
      serieName: props.name,
      dateFormat,
    })
    isChartInitialized = true
  }
  if (chartRef.value.clientWidth > 0 && chartRef.value.clientHeight > 0) {
    init()
  }
  resizeObserver = new ResizeObserver((entries) => {
    if (chartRef.value.clientWidth > 0 && chartRef.value.clientHeight) {
      if (!isChartInitialized) {
        init()
      }
      else {
        chartInstance.resize()
      }
    }
  })
  resizeObserver.observe(chartRef.value)
})

onUnmounted(() => {
  if (chartInstance) {
    chartInstance.dispose()
    chartInstance = null
  }
})
</script>

<style scoped>
.chart-main {
  flex: 1;
  min-height: 0;
}
</style>
