import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.goldandgoblins.app',
  appName: 'Gold and Goblins',
  webDir: 'dist',
  backgroundColor: '#1a1410',
  ios: {
    contentInset: 'always',
  },
  android: {
    backgroundColor: '#1a1410',
  },
};

export default config;
