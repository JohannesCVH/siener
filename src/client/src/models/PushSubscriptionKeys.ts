export class PushSubscriptionKeys {
    p256dh: string;
    auth: string;
    
    constructor(p256dh: string, auth: string) {
        this.p256dh = p256dh;
        this.auth = auth;
    }
}