import PagBankProviderService from "./service"
import { Module } from "@medusajs/framework/utils"

export const PAGBANK_MODULE = "pagbank_payment"

export default Module(PAGBANK_MODULE, {
  service: PagBankProviderService,
})

export * from "./models/pagbank-payment"

