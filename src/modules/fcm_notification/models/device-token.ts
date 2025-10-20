import { model } from "@medusajs/framework/utils"

export enum DevicePlatform {
  ANDROID = "android",
  IOS = "ios"
}

const DeviceToken = model.define("device_token", {
  id: model.id().primaryKey(),
  
  fcm_token: model.text().unique(),
  
  user_id: model.text().nullable(),
  customer_id: model.text().nullable(),
  
  platform: model.enum(DevicePlatform),
  device_name: model.text().nullable(),
  app_version: model.text().nullable(),
  
  is_active: model.boolean().default(true),
  last_used_at: model.dateTime().nullable(),
})

export default DeviceToken

