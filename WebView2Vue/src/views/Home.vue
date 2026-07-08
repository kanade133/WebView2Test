<template>
  <div class="container">
    <div>
      <el-button :disabled="isStart" size="small" @click="handleStart">开始</el-button>
      <el-button :disabled="!isStart" size="small" @click="handleStop">停止</el-button>
    </div>
    <el-tabs v-model="activeName" class="tabs" type="card">
      <el-tab-pane label="配置" name="config" class="config-tab-pane">
        <el-card header="行情" class="config-detail-card">
          <el-scrollbar>
            <div class="card-body-container">
              <el-form :model="marketCollector" size="small">
                <el-form-item label="平台">
                  <el-radio-group v-model="marketCollector.platform">
                    <el-radio value="Mock" disabled>Mock</el-radio>
                    <el-radio value="Rithmic">Rithmic</el-radio>
                  </el-radio-group>
                </el-form-item>
                <el-form-item label="交易时间">
                  <el-radio-group v-model="marketCollector.tradingTimeType">
                    <el-radio :value="0" disabled>全天</el-radio>
                    <el-radio :value="1">外盘</el-radio>
                  </el-radio-group>
                </el-form-item>
                <el-form-item label="用户">
                  <el-input v-model="marketCollector.user" size="small" />
                </el-form-item>
                <el-form-item label="密码">
                  <el-input v-model="marketCollector.password" size="small" show-password />
                </el-form-item>
              </el-form>
              <label>合约列表</label>
              <el-table :data="marketCollector.symbols" size="small">
                <el-table-column prop="exchange" label="交易所">
                  <template #default="{ row }">
                    <el-input v-model="row.exchange" size="small" />
                  </template>
                </el-table-column>
                <el-table-column prop="mainSymbol" label="订阅合约">
                  <template #default="{ row }">
                    <el-input v-model="row.mainSymbol" size="small" />
                  </template>
                </el-table-column>
                <el-table-column prop="mappingSymbol" label="映射合约">
                  <template #default="{ row }">
                    <el-input v-model="row.mappingSymbol" size="small" />
                  </template>
                </el-table-column>
                <el-table-column label="操作" width="40">
                  <template #header>
                    <el-button type="primary" circle :icon="Plus" size="small" @click="marketCollector.symbols.push({
                      exchange: '',
                      mainSymbol: '',
                      mappingSymbol: '',
                    })" />
                  </template>
                  <template #default="{ $index }">
                    <el-button type="danger" circle :icon="Delete" size="small"
                      @click="marketCollector.symbols.splice($index, 1)" />
                  </template>
                </el-table-column>
              </el-table>
            </div>
          </el-scrollbar>
        </el-card>
        <el-card header="策略" class="config-detail-card">
          <el-scrollbar>
            <div class="card-body-container">
              <el-form :model="strategy" size="small">
                <el-form-item label="类型">
                  <el-radio-group v-model="strategy.type">
                    <el-radio value="NetRange">周期范围</el-radio>
                  </el-radio-group>
                </el-form-item>
                <el-form-item label="信号合约">
                  <el-input v-model="strategy.signalSymbol" />
                </el-form-item>
                <el-form-item label="周期毫秒">
                  <el-input-number v-model="strategy.periodMilliseconds" :min="0" />
                </el-form-item>
                <el-form-item label="价格范围">
                  <el-input-number v-model="strategy.range" :min="0" />
                </el-form-item>
              </el-form>
              <div style="display: flex; gap: 10px;">
                <label>下单列表</label>
                <el-button type="primary" circle :icon="Plus" size="small" @click="strategy.orders.push({
                  openUser: '',
                  openSymbol: '',
                  openVolume: 1,
                  lockAfterMilliseconds: 1000,
                  lockUser: '',
                  lockSymbol: '',
                  lockVolume: 1,
                })" />
              </div>
              <el-card v-for="(item, index) in strategy.orders" class="item-card">
                <div class="card-body-container">
                  <div style="display: flex; justify-content: flex-end;">
                    <el-button type="danger" circle :icon="Minus" size="small"
                      @click="strategy.orders.splice(index, 1)" />
                  </div>
                  <el-form size="small">
                    <el-form-item label="开仓用户">
                      <el-input v-model="item.openUser" />
                    </el-form-item>
                    <el-form-item label="开仓合约">
                      <el-input v-model="item.openSymbol" />
                    </el-form-item>
                    <el-form-item label="开仓手数">
                      <el-input-number v-model="item.openVolume" :min="0" />
                    </el-form-item>
                    <el-form-item label="开仓后等待锁仓毫秒">
                      <el-input-number v-model="item.lockAfterMilliseconds" :min="0" />
                    </el-form-item>
                    <el-form-item label="锁仓用户">
                      <el-input v-model="item.lockUser" />
                    </el-form-item>
                    <el-form-item label="锁仓合约">
                      <el-input v-model="item.lockSymbol" />
                    </el-form-item>
                    <el-form-item label="锁仓手数">
                      <el-input-number v-model="item.lockVolume" :min="0" />
                    </el-form-item>
                  </el-form>
                </div>
              </el-card>
            </div>
          </el-scrollbar>
        </el-card>
        <el-card header="交易账号" class="config-detail-card">
          <template #header>
            交易账号
            <el-button type="primary" circle :icon="Plus" size="small" @click="tradeUsers.push({
              platform: 'MT5',
              userName: '',
              password: '',
              serverFileName: '',
              symbols: [],
            })" />
          </template>
          <el-scrollbar>
            <div class="card-body-container">
              <el-card v-for="(tradeUser, index) in tradeUsers" class="item-card">
                <div class="card-body-container">
                  <div style="display: flex; justify-content: flex-end;">
                    <el-button type="danger" circle :icon="Minus" size="small" @click="tradeUsers.splice(index, 1)" />
                  </div>
                  <el-form size="small">
                    <el-form-item label="平台">
                      <el-radio-group v-model="tradeUser.platform" size="small">
                        <el-radio value="MT4">MT4</el-radio>
                        <el-radio value="MT5">MT5</el-radio>
                      </el-radio-group>
                    </el-form-item>
                    <el-form-item label="用户">
                      <el-input v-model="tradeUser.userName" size="small" />
                    </el-form-item>
                    <el-form-item label="密码">
                      <el-input v-model="tradeUser.password" size="small" show-password />
                    </el-form-item>
                    <el-form-item label="服务器文件">
                      <el-select v-model="tradeUser.serverFileName" size="small">
                        <el-option v-for="value in serverFileNames" :label="value" :value="value" />
                      </el-select>
                    </el-form-item>
                  </el-form>
                  <div style="display: flex; gap: 10px;">
                    <label>关联合约</label>
                    <el-button type="primary" circle :icon="Plus" size="small" @click="tradeUser.symbols.push('')" />
                  </div>
                  <div v-for="(item, index) in tradeUser.symbols" class="relative-symbol-item">
                    <el-input v-model="tradeUser.symbols[index]" size="small" />
                    <el-button type="danger" circle :icon="Delete" size="small"
                      @click="tradeUser.symbols.splice(index, 1)" />
                  </div>
                </div>
              </el-card>
            </div>
          </el-scrollbar>
        </el-card>
      </el-tab-pane>
      <el-tab-pane label="交易账号下单记录" name="tradeUserOrderRecord">
        <el-scrollbar class="trade-user-orders-scrollbar">
          <div class="trade-user-orders-container">
            <el-card v-for="item in tradeUserOrders" :header="item.user" class="trade-user-orders-item-card">
              <el-table :data="item.orders" size="small" height="100%">
                <el-table-column label="时间" width="140">
                  <template #default="{ row }">
                    {{ formatDateTime(row.dateTime) }}
                  </template>
                </el-table-column>
                <el-table-column prop="id" label="Id" width="90" />
                <el-table-column prop="symbol" label="合约" />
                <el-table-column label="买卖" width="40">
                  <template #default="{ row }">
                    {{ formatBuySell(row.buySell) }}
                  </template>
                </el-table-column>
                <el-table-column label="开平" width="40">
                  <template #default="{ row }">
                    {{ formatOpenClose(row.openClose) }}
                  </template>
                </el-table-column>
                <el-table-column label="手数" width="60">
                  <template #default="{ row }">
                    {{ formatVolume(row.volume) }}
                  </template>
                </el-table-column>
              </el-table>
            </el-card>
          </div>
        </el-scrollbar>
      </el-tab-pane>
    </el-tabs>
    <!-- <el-card header="行情" class="tick-card">
      <ChartView :data="tickData" />
    </el-card> -->
    <el-card header="日志" class="log-card">
      <el-scrollbar>
        <div v-for="value in logMessages">
          <el-text> {{ value }} </el-text>
        </div>
      </el-scrollbar>
    </el-card>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, onUnmounted } from 'vue';
import { Plus, Minus, Delete } from '@element-plus/icons-vue'
import ChartView from './Chart/index.vue'
import dayjs from 'dayjs'

const frame = window.chrome?.webview?.hostObjects?.frame
const isStart = ref(false)
const activeName = ref('config')
const logMaxCount = 100
const logMessages = reactive([])
const marketCollector = ref({
  platform: 'Rithmic',
  tradingTimeType: 1,
  user: '31670375',
  password: '5206218',
  symbols: [
    {
      exchange: 'COMEX',
      mainSymbol: 'GC',
      mappingSymbol: 'GC',
    }
  ],
})
const strategy = ref({
  type: 'NetRange',
  signalSymbol: 'GC',
  periodMilliseconds: 5000,
  range: 0.4,
  orders: [
    {
      openUser: '167552134',
      openSymbol: 'GOLD_',
      openVolume: 2,
      lockAfterMilliseconds: 5000,
      lockUser: '78348962',
      lockSymbol: 'GOLD',
      lockVolume: 1,
    },
  ],
})
const tradeUsers = ref([
  {
    platform: 'MT5',
    userName: '167552134',
    password: '',
    serverFileName: 'myTest.dat2',
    symbols: ['GOLD_']
  },
  {
    platform: 'MT4',
    userName: '78348962',
    password: '',
    serverFileName: 'myTest3.dat2',
    symbols: ['GOLD']
  },
])
const serverFileNames = ref([])
const tradeUserOrders = ref([])
const tickData = ref([])

const formatDateTime = value => {
  return value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '-'
}

const formatBuySell = value => {
  switch (value) {
    case 0: return '买入'
    case 1: return '卖出'
    default: return '未知'
  }
}

const formatOpenClose = value => {
  switch (value) {
    case 0: return '开仓'
    case 1: return '平仓'
    default: return '未知'
  }
}
const formatVolume = (volume) => {
  if (volume === 0) return 0
  return volume ? volume.toFixed(2) : '-';
}

const getHubConfigs = () => {
  return {
    marketCollectors: [
      marketCollector.value,
    ],
    strategies: [
      strategy.value,
    ],
    tradeUsers: tradeUsers.value,
  }
}

const handleStart = async () => {
  isStart.value = true
  try {
    const hubConfigs = getHubConfigs()
    tradeUserOrders.value = hubConfigs.tradeUsers.map(a => ({
      user: a.userName,
      orders: [],
    }))
    await frame?.Start(JSON.stringify(hubConfigs))
    activeName.value = 'tradeUserOrderRecord'
  } catch (e) {
    console.log(e)
    isStart.value = false
  }
}

const handleStop = () => {
  isStart.value = false
  frame?.Stop()
}

const handleLogMessage = message => {
  logMessages.unshift(message)
  if (logMessages.length > logMaxCount) {
    logMessages.pop()
  }
}

const handleOrderMessage = order => {
  const item = tradeUserOrders.value.find(a => a.user === order.user)
  if (item) {
    item.orders.unshift(order)
  }
}

const handleTickMessage = tick => {
  // tickData.value.push([tick.dateTime, tick.price])
}

const handleMessage = arg => {
  switch (arg.data.type) {
    case 'OnLog': handleLogMessage(arg.data.data); break;
    case 'OnOrder': handleOrderMessage(arg.data.data); break;
    case 'OnTick': handleTickMessage(arg.data.data); break;
    default: break;
  }
}

onMounted(async () => {
  if (window.chrome?.webview) {
    window.chrome.webview.addEventListener('message', handleMessage)
    serverFileNames.value = await frame.GetServerNames()
    const json = await frame.GetHubConfigs()
    const hubConfigs = JSON.parse(json)
    marketCollector.value = hubConfigs.marketCollectors[0]
    strategy.value = hubConfigs.strategies[0]
    tradeUsers.value = hubConfigs.tradeUsers
  }
})

onUnmounted(() => {
  if (window.chrome?.webview) {
    window.chrome.webview.removeEventListener('message', handleMessage)
  }
})
</script>

<style scoped>
.container {
  height: 100%;
  display: flex;
  flex-direction: column;
  gap: 10px;
  box-sizing: border-box;
  padding: 10px;
}

.el-tabs {
  flex: 1;
  min-height: 0px;
}

.el-tabs :deep(.el-tabs__content) {
  height: 100%;
}

.el-tab-pane {
  height: 100%;
}

.config-tab-pane {
  height: 100%;
  display: flex;
  justify-content: space-between;
  gap: 10px;
}

.config-detail-card {
  flex: 1;
  min-width: 0px;
}

.config-detail-card :deep(.el-card__header) {
  padding: 10px;
}

.config-detail-card :deep(.el-card__body) {
  padding: 0px;
}

.card-body-container {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: 10px;
}

.item-card :deep(.el-card__body) {
  padding: 0px;
}

.tick-card {
  height: 300px;
}

.tick-card :deep(.el-card__body) {
  display: flex;
}

.relative-symbol-item {
  display: flex;
  gap: 10px;
}

.trade-user-orders-scrollbar :deep(.el-scrollbar__view) {
  height: 100%;
}

.trade-user-orders-container {
  height: 100%;
  display: flex;
  width: fit-content;
  gap: 10px;
}

.trade-user-orders-item-card {
  height: 100%;
  box-sizing: border-box;
}

.trade-user-orders-item-card :deep(.el-card__header) {
  padding: 10px;
}

.trade-user-orders-item-card :deep(.el-card__body) {
  height: 100%;
  padding: 10px;
}

.log-card {
  height: 300px;
}

.log-card :deep(.el-card__header) {
  padding: 10px;
}

.log-card :deep(.el-card__body) {
  padding: 10px;
}
</style>