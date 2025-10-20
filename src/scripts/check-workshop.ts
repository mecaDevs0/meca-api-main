import { OFICINA_MODULE } from "../modules/oficina"

export default async function checkWorkshop({ container }: any) {
  const oficinaService = container.resolve(OFICINA_MODULE)
  
  const oficinas = await oficinaService.listOficinas({ email: "secex@bb.com.br" })
  
  if (oficinas.length > 0) {
    const oficina = oficinas[0]
    console.log("Oficina encontrada:")
    console.log("ID:", oficina.id)
    console.log("Nome:", oficina.name)
    console.log("Email:", oficina.email)
    console.log("Metadata:", JSON.stringify(oficina.metadata, null, 2))
  } else {
    console.log("Oficina não encontrada")
  }
}

