<template>
    <div class="container">
        <div class="row">
            <div class="col-12">
                <div class="stream-container d-flex justify-content-center">
                    <video v-show="streamReady" ref="streamElRef" @playing="streamReady = true" class="w-100" autoplay muted playsinline webkit-playsinline></video>
                    <span v-show="!streamReady" class="align-self-center">Loading stream..</span>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts" setup>
    import { inject, onMounted, onUnmounted, ref } from 'vue';

    const config: any = inject('config');

    const props = defineProps({
        cameraName: {
            type: String
        }
    });
    
    const streamElRef = ref(null);
    let streamReady = ref(false);

    onMounted(async () => {
        await startStream();
    });

    

    onUnmounted(async () => {
        
    });

    const startStream = async () => {
        const rtcPeerConn = new RTCPeerConnection({});

        rtcPeerConn.ontrack = async (event) => {
            const stream = event.streams[0];
            const streamEl = streamElRef.value;
            
            if (streamEl.srcObject !== stream) {
                streamEl.srcObject = stream;
            }

            await streamElRef.value.play().catch(console.error);
        };

        rtcPeerConn.addTransceiver('video', { direction: 'recvonly' });
        rtcPeerConn.addTransceiver('audio', { direction: 'recvonly' });

        const offer = await rtcPeerConn.createOffer();
        await rtcPeerConn.setLocalDescription(offer);

        const offerRes = await fetch(`${config.API_URL}:${config.WEBRTC_HTTP_PORT}/${props.cameraName}/whep`, {
            method: 'POST',
            body: offer.sdp,
            headers: { 'Content-Type': 'application/sdp' },
            mode: 'cors'
        });

        if (!offerRes.ok) console.error('WHEP request failed.');

        const answerSdp = await offerRes.text();
        await rtcPeerConn.setRemoteDescription(new RTCSessionDescription({
            type: 'answer',
            sdp: answerSdp
        })); 
    };
</script>

<style scoped>
    
</style>