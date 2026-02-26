<template>
  <div class="site-container">
    <nav v-if="($route.name != 'Login' && $route.name != 'Register')" class="navbar mb-4">
      <div class="container-fluid">
        <router-link class="navbar-brand" to="/">
          <img src="./assets/logo.png" style="max-width: 48px; box-shadow: 0 4px 8px rgba(0,0,0,0.2); border-radius: 4px;"/>
        </router-link>

        <button @click="subscribeToPush" :disabled="isSubscribed" class="btn btn-primary">
          {{ isSubscribed ? 'Push Notfications Enabled' : 'Enable Push Notfications' }}
        </button>

        <button @click="logout" class="btn btn-primary">
          Logout
        </button>
        
      </div>
    </nav>
    <div class="container">
      <router-view></router-view>
    </div>

    <toast ref="toast"></toast>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, inject, onMounted, provide } from 'vue';
  import { useRouter } from 'vue-router';
  import Toast from './components/Toast.vue';
  import Cookies from 'js-cookie'
  import { ToastType } from './models/ToastTypes';
  import { PushSubscription as PushSubscription } from './models/PushSubscription';
  import { PushSubscriptionKeys } from './models/PushSubscriptionKeys';

  const config: any = inject('config');

  const router = useRouter();
  const siteBgColor = ref('#ece5d8');
  const toast = ref(null);

  const isSubscribed = ref(false);
  const isPhone = computed(() => {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
  });
  console.log(`IsMobile: ${isPhone.value}`);
  console.log(`API_URL: ${config.API_URL}`);
  console.log(`${config.API_URL}/PushNotification/SaveSubscription`);

  onMounted(async () => {
    
  });

  provide('makeToast', (title, message, type: ToastType) => {
    if (toast.value) {
      toast.value.show(title, message, type);
    }
  });

  const subscribeToPush = async () => {
    try {
      const registration = await navigator.serviceWorker.ready;

      const permission = await Notification.requestPermission();
      if (permission !== 'granted') return;

      const existingSubscription = await registration.pushManager.getSubscription();

      if (existingSubscription) {
        await existingSubscription.unsubscribe();
        console.log('Unsubscribed from the old key.');
      }
      
      const subscriptionRes = await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: config.VAPID_PUBLIC_KEY
      });
      debugger

      const decoder = new TextDecoder('utf-8');
      const pushSubscription: PushSubscription = new PushSubscription(
        Cookies.get('User.Id'), 
        subscriptionRes.endpoint,
        new PushSubscriptionKeys(
          btoa(String.fromCharCode.apply(null, new Uint8Array(subscriptionRes.getKey('p256dh')))),
          btoa(String.fromCharCode.apply(null, new Uint8Array(subscriptionRes.getKey('auth'))))
        )
      );

      await fetch(`${config.API_URL}:${config.API_PORT}/api/PushNotification/SaveSubscription`, {
        method: 'POST',
        body: JSON.stringify(pushSubscription),
        headers: { 'Content-Type': 'application/json' }
      });

      isSubscribed.value = true;
    }
    catch(error) {
      console.error('Push subscription failed: ', error);
    }
  }

  const logout = async () => {
    Cookies.remove('User.Id');
    router.push({ name: 'Login' });
  };
</script>

<style scoped>
  .site-container {
    background-color: v-bind(siteBgColor);
    height: 100vh;
  }
</style>