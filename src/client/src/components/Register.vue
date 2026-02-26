<template>
    <div class="d-flex justify-content-center align-items-center min-vh-100">
        <div class="card shadow col-6 px-4">
            <div class="card-body">
                <h3 class="text-center mb-4">Register</h3>

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

                <div>
                    <button @click="register()" class="btn btn-primary w-100" :disabled="isWorking">{{ registerBtnTxt }}</button>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
    import axios from 'axios';
    import { useRoute, useRouter } from 'vue-router';
    import { onMounted, ref, inject, watch } from 'vue';
    import Cookies from 'js-cookie'
    import { User } from '../models/User';
    import { ToastType } from '../models/ToastTypes';

    const config: any = inject('config');
    const route = useRoute();
    const router = useRouter();
    const makeToast = inject<(title: string, message: string, type: ToastType) => void>('makeToast');

    const username = ref('');
    const registerBtnTxt = ref('Register');
    const isWorking = ref(false);

    onMounted(() => {
        
    });

    const register = async () => {
        registerBtnTxt.value = 'Registering..'
        isWorking.value = true;

        const user: User = new User(crypto.randomUUID(), username.value);
        
        try {
            const response = await axios.post(
                `${config.API_URL}:${config.API_PORT}/api/User/Save`,
                user
            );
            
            if (response.status == 201) {
                makeToast('Registration Successful', 'User registered successfully!', ToastType.Sucess);
                Cookies.set('User.Id', user.id);
                router.push({ name: 'Dashboard' });
            }
        } catch(error) {
            debugger
            console.error(error);
            if (error.status == 500) {
                makeToast('Error', 'Grave server error occurred.', ToastType.Error);
            }
        } finally {
            setTimeout(() => {
                registerBtnTxt.value = 'Register'
                isWorking.value = false;
            }, 2000);
        }
    };
</script>

<style scoped>
    
</style>