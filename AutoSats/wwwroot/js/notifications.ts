export async function requestSubscription(key) {
    const worker = await navigator.serviceWorker.getRegistration();
    let subscription = await worker.pushManager.getSubscription();
    if (!subscription) {
        subscription = await subscribe(worker, key);
        if (!subscription) {
            return null;
        }
    }

    const json = subscription.toJSON();

    return {
        url: subscription.endpoint,
        p256dh: json.keys['p256dh'],
        auth: json.keys['auth']
    };
}

async function subscribe(worker, key) {
    try {
        return await worker.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: key
        });
    } catch (error) {
        console.log(error);
        return null;
    }
}