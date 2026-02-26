import { PushSubscriptionKeys } from "./PushSubscriptionKeys";

export class PushSubscription {
    userId: number;
    endpoint: string;
    keys: PushSubscriptionKeys;

    constructor(userId: number, endpoint: string, keys: PushSubscriptionKeys) {
        this.userId = userId;
        this.endpoint = endpoint;
        this.keys = keys;
    }
}