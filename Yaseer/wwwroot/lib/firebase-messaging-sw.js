// Compat SDKs inside the SW
importScripts("https://www.gstatic.com/firebasejs/10.12.2/firebase-app-compat.js");
importScripts("https://www.gstatic.com/firebasejs/10.12.2/firebase-messaging-compat.js");

firebase.initializeApp({
    apiKey: "AIzaSyCPeR8WOTjlUXBSvvbKA3qElVGB33fJLwg",
    authDomain: "websitenotification-53e82.firebaseapp.com",
    projectId: "websitenotification-53e82",
    storageBucket: "websitenotification-53e82.appspot.com",
    messagingSenderId: "720780980996",
    appId: "1:720780980996:web:df5d3b0b35373c3b409651"
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
    const title = payload?.notification?.title || "New message";
    const options = { body: payload?.notification?.body || "" };
    self.registration.showNotification(title, options);
});