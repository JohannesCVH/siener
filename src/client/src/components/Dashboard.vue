<template>
    <div class="container">
        <div class="row">
            <div v-if="camerasRef.length" v-for="camera in camerasRef" class="col-12 col-md-6">
                <div class="card clickable-card mb-2" @click="navToStream(camera.name)">
                    <img v-if="camera.thumbnailUrl" :src="camera.thumbnailUrl" />
                    <span v-else>Loading thumbnail..</span>
                    <div class="card-body">
                        <h5 class="card-title text-center">{{ camera.name }}</h5>
                    </div>
                </div>
            </div>
            <div v-else>Loading dashboard..</div>
        </div>
    </div>
</template>

<script lang="ts" setup>
    import axios from 'axios';
    import { useRouter } from 'vue-router';
    import { Camera } from '../models/Camera';
    import { inject, onMounted, ref } from 'vue';

    const config: any = inject('config');

    const router = useRouter();

    const camerasRef = ref<Camera[]>([]);

    const fetchStreams = async (): Promise<void> => {
        try {
            const res = await axios.get<Camera[]>(`${config.API_URL}:${config.API_PORT}/api/Dashboard/Streams`);
            
            res.data.forEach(camera => {
                camerasRef.value.push(new Camera(camera.name))
            });
        }
        catch (err) {
            console.error(`fetchStreams() ERROR: ${err}`);
        }
        finally {
            fetchThumbnails();
        }
    };

    const fetchThumbnail = async (cameraName: string): Promise<Blob> => {
        try {
            const res = await axios.get<Blob>(`${config.API_URL}:${config.API_PORT}/api/Dashboard/GetThumbnail/${cameraName}`, {
                responseType: 'blob'
            });

            return res.data;
        }
        catch(error) {
            console.error(`fetchThumbnail() ERROR: ${error}`)
        }
    };

    const fetchThumbnails = async (): Promise<void> => {
        const promises = camerasRef.value.map(async (cam) => {
            const res = await fetchThumbnail(cam.name);
            cam.thumbnailUrl = URL.createObjectURL(res);
        });
        
        await Promise.all(promises);
    };

    const navToStream = async (cameraName: string) => {
        router.push({
            name: 'Stream',
            params: { cameraName: cameraName }
        })
    }

    onMounted(async () => {
        await fetchStreams();
    });
</script>

<style scoped>
    .clickable-card {
        transition: all 0.2s ease;
        cursor: pointer;
    }

    .clickable-card:hover {
        transform: translateY(-3px);
        box-shadow: 0 4px 8px rgba(0,0,0,0.2) !important;
    }
</style>