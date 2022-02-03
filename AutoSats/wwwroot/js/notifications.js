var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
export function requestSubscription(key) {
    return __awaiter(this, void 0, void 0, function* () {
        const worker = yield navigator.serviceWorker.getRegistration();
        let subscription = yield worker.pushManager.getSubscription();
        if (!subscription) {
            subscription = yield subscribe(worker, key);
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
    });
}
export function checkServiceWorkerExists() {
    return !!navigator.serviceWorker;
}
function subscribe(worker, key) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            return yield worker.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: key
            });
        }
        catch (error) {
            console.log(error);
            return null;
        }
    });
}
//# sourceMappingURL=notifications.js.map