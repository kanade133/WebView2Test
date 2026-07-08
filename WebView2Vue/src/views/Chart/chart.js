import * as echarts from 'echarts'
import dayjs from 'dayjs'

class ChartInstance {
  constructor(dom) {
    this.digits = 2
    this.blankPercent = 0.2
    this.showCount = 100
    this.secondaryGridHeight = 15
    this._dataArray = []
    this._serieName = ''
    this._dateFormat = 'YYYY/MM/DD\nHH:mm'
    this._showBlank = true

    this.chart = echarts.init(dom, 'dark')
    this.chart.setOption({
      animation: false,
      tooltip: {
        trigger: 'axis',
        axisPointer: {
          type: 'cross',
        },
        hideDelay: 0,
        transitionDuration: 0,
        displayTransition: false,
      },
      axisPointer: {
        link: [
          {
            xAxisIndex: 'all'
          }
        ],
      },
    })
  }

  update({
    dataArray,
    serieName,
    dateFormat,
    showBlank,
    updateDataZoom,
    dataZoomStart,
    dataZoomEnd,
  }) {
    if (dataArray !== undefined && dataArray !== null) {
      this._dataArray = dataArray
    }
    if (serieName !== undefined && serieName !== null) {
      this._serieName = serieName
    }
    if (dateFormat !== undefined && dateFormat !== null) {
      this._dateFormat = dateFormat
    }
    if (showBlank !== undefined && showBlank !== null) {
      this._showBlank = showBlank
    }
    this.chart.setOption(this.#buildChartOption(updateDataZoom, dataZoomStart, dataZoomEnd))
  }

  dispose() {
    this.chart.dispose()
  }

  resize() {
    this.chart.resize()
  }

  getNewStartEnd(start, end, dataCount, addPrevCount, addNextCount) {
    if (dataCount === 0) {
      return { start: 0, end: 100 }
    }
    const oldBlank = this._showBlank ? dataCount * this.blankPercent : 0;
    const oldTotal = dataCount + oldBlank;
    const oldStartIndex = (start / 100) * oldTotal;
    const oldEndIndex = (end / 100) * oldTotal;
    const oldEndBlank = this._showBlank && oldEndIndex > dataCount ? oldEndIndex - dataCount : 0;
    const newDataCount = dataCount + addPrevCount + addNextCount;
    const newBlank = this._showBlank ? newDataCount * this.blankPercent : 0;
    const newTotal = newDataCount + newBlank;
    const newStartIndex = oldStartIndex + addPrevCount;
    const newEndBlank = this._showBlank ? oldEndBlank / oldBlank * newBlank : 0;
    const newEndIndex = oldEndIndex + addPrevCount - oldEndBlank + newEndBlank;
    const newStart = (newStartIndex / newTotal) * 100;
    const newEnd = (newEndIndex / newTotal) * 100;
    return { newStart, newEnd };
  }

  getStartEnd(startIndex, endIndex, dataCount, showBlank) {
    const blank = showBlank ? dataCount * this.blankPercent : 0;
    const total = dataCount + blank;
    const start = (startIndex / total) * 100;
    const end = (endIndex / total) * 100;
    return { start, end };
  }

  #buildChartOption(updateDataZoom, dataZoomStart, dataZoomEnd) {
    const { dates, data } = this.#processChartData(this._dataArray, this._dateFormat)
    // 添加主图
    const series = [this.#buildTickSeries(this._serieName, data)]
    const xAxis = [this.#buildPrimaryXAxis(dates, this._showBlank)]
    const yAxis = [this.#buildPrimaryYAxis()]
    const grids = [this.#buildPrimaryGrid(0)]
    const titleTexts = []
    const titleRichs = []
    const dataZoomXAxisIndex = [0]
    const titles = [this.#buildPrimaryTitle(titleTexts.join(' '), Object.assign({}, ...titleRichs))]
    return {
      title: titles,
      xAxis: xAxis,
      yAxis: yAxis,
      grid: grids,
      series: series,
      // dataZoom: this.#buildDataZoom(dates.length, dataZoomXAxisIndex, this._showBlank, updateDataZoom, dataZoomStart, dataZoomEnd),
    }
  }

  #processChartData(dataArray, dateFormat) {
    const dates = []
    const data = []
    for (const [timestamp, value] of dataArray) {
      dates.push(timestamp)
      data.push([timestamp, +value.toFixed(this.digits)])
    }
    return { dates, data }
  }

  #buildTickSeries(serieName, data) {
    const markLineData = []
    if (data.length > 0) {
      markLineData.push({
        name: 'latest close',
        yAxis: data[data.length - 1][1],
        z2: 2,
      })
    }
    return {
      type: 'line',
      name: serieName,
      data: data,
      markLine: {
        symbol: ['none', 'none'],
        label: {
          position: 'insideEndBottom',
          color: '#ffffff',
          distance: [0, -10],
          backgroundColor: 'inherit',
          padding: [5, 10],
          borderRadius: 2,
        },
        data: markLineData,
      },
    }
  }

  #buildDataZoom(dataCount, xAxisIndex, showBlank, updateDataZoom, start, end) {
    if (!dataCount || !updateDataZoom) {
      return { type: 'inside', xAxisIndex: xAxisIndex }
    }
    if (start !== null && start !== undefined && end !== null && end !== undefined) {
      return { type: 'inside', xAxisIndex: xAxisIndex, start: start, end: end }
    }
    if (dataCount > this.showCount) {
      const startIndex = dataCount - this.showCount
      const totalLength = showBlank ? (dataCount + dataCount * this.blankPercent) : dataCount
      const endIndex = showBlank ? (dataCount + this.showCount * this.blankPercent) : dataCount
      return { type: 'inside', xAxisIndex: xAxisIndex, start: startIndex / totalLength * 100, end: endIndex / totalLength * 100 }
    }
    return { type: 'inside', xAxisIndex: xAxisIndex, start: 0, end: 100 }
  }

  #buildPrimaryXAxis(data, showBlank, secondaryGridCount) {
    return {
      gridIndex: 0,
      type: 'time',
      // max: showBlank ? (value) => (value.max + 1) * (1 + this.blankPercent) - 1 : null,
      // data: data,
      splitLine: {
        show: false
      },
      axisLabel: {
        showMinLabel: true,
        alignMinLabel: true,
        show: true,
        inside: secondaryGridCount > 0,
      },
    }
  }

  #buildPrimaryYAxis() {
    return {
      scale: true,
      position: 'right',
      axisLabel: {
        showMaxLabel: false,
        inside: true,
      },
      splitLine: {
        show: false,
      },
    }
  }

  #buildPrimaryGrid(secondaryGridCount) {
    return {
      left: 0,
      right: 0,
      top: 0,
      bottom: `${this.secondaryGridHeight * secondaryGridCount}%`,
      outerBoundsMode: 'same',
    }
  }

  #buildPrimaryTitle(text, rich) {
    return {
      right: 0,
      top: 0,
      text,
      textStyle: {
        fontWeight: 'normal',
        fontSize: 12,
        rich,
      },
    }
  }
}

export default ChartInstance