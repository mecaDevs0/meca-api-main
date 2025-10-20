import FcmNotificationService from "./service"
import { Module } from "@medusajs/framework/utils"

export const FCM_MODULE = "fcm_notification"

export default Module(FCM_MODULE, {
  service: FcmNotificationService,
})

export * from "./models/device-token"

