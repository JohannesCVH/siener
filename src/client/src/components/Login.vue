<template>
    <div class="d-flex justify-content-center align-items-center min-vh-100">
        <div class="card shadow col-12 col-md-6 px-4">
            <div class="card-body">
                <h3 class="text-center mb-4">Login</h3>

                <div class="mb-3">
                    <label for="username-input" class="block mb-2 me-2">Enter your username:</label>
                    <input 
                        v-model="username" 
                        id="username-input"
                        type="text" 
                        placeholder="E.g. john@doe.com"
                        class="border p-2 w-100"
                    />
                </div>

                <div class="mb-4">
                    <button @click="login()" class="btn btn-primary w-100" :disabled="isWorking">{{ loginBtnTxt }}</button>
                </div>
                <div class="">
                    <span>
                        Don't have an account?
                        <router-link :to="{ name: 'Register' }">Sign Up</router-link>
                    </span>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
    import axios from 'axios';
    import { useRoute, useRouter } from 'vue-router';
    import { onMounted, ref, inject } from 'vue';
    import Cookies from 'js-cookie'
    import { LoginRequest } from '../models/LoginRequest';
    import { User } from '../models/User';
    import { ToastType } from '../models/ToastTypes';

    const config: any = inject('config');
    const route = useRoute();
    const router = useRouter();
    const makeToast = inject<(title: string, message: string, type: ToastType) => void>('makeToast');

    const username = ref('');
    const loginBtnTxt = ref('Login');
    const isWorking = ref(false);

    onMounted(() => {
        
    });

    const login = async () => {
        loginBtnTxt.value = 'Logging in..'
        isWorking.value = true;

        const loginRequest = new LoginRequest(username.value);
        
        try {
            const response = await axios.post<User>(
                `${config.API_URL}:${config.API_PORT}/api/User/Login`,
                loginRequest
            );
            
            if (response.status == 200) {
                Cookies.set('User.Id', response.data.id);
                router.push({ name: 'Dashboard' });
                makeToast('Success', 'Successfully logged in', ToastType.Sucess);
            }
        } catch(error) {
            if (error.status == 401) {
                makeToast('Error', 'Username incorrect', ToastType.Error);
            }
            console.error(error);
        } finally {
            setTimeout(() => {
                loginBtnTxt.value = 'Login'
                isWorking.value = false;
            }, 2000);
        }
    };
</script>

<style scoped>
    
</style>