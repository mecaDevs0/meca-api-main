import { MedusaRequest, MedusaResponse } from "@medusajs/framework/http"
import { VEHICLE_MODULE } from "../../../modules/vehicle"

export const AUTHENTICATE = true

/**
 * GET /store/vehicles
 *
 * Listar veículos do cliente logado
 */
export async function GET(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const vehicleModuleService = req.scope.resolve(VEHICLE_MODULE)
  
  try {
    const vehicles = await vehicleModuleService.listVehicles({
      customer_id: req.auth_context.customer_id
    })

    const formattedVehicles = vehicles.map(vehicle => ({
      id: vehicle.id,
      plate: vehicle.plate,
      brand: vehicle.brand,
      model: vehicle.model,
      year: vehicle.year,
      color: vehicle.color,
      fuel: vehicle.fuel,
      is_primary: vehicle.is_primary,
      created_at: vehicle.created_at,
      updated_at: vehicle.updated_at
    }))

    return res.json({
      vehicles: formattedVehicles,
      total: formattedVehicles.length
    })

  } catch (error) {
    console.error("Erro ao buscar veículos:", error)
    return res.status(500).json({
      message: "Erro ao buscar veículos",
      error: error.message
    })
  }
}

/**
 * POST /store/vehicles
 *
 * Adicionar novo veículo para o cliente logado
 */
export async function POST(
  req: MedusaRequest,
  res: MedusaResponse
) {
  const vehicleModuleService = req.scope.resolve(VEHICLE_MODULE)
  
  const {
    plate,
    brand,
    model,
    year,
    color,
    fuel,
    is_primary = false
  } = req.body

  // Validações básicas
  if (!plate || !brand || !model || !year) {
    return res.status(400).json({
      message: "Placa, marca, modelo e ano são obrigatórios"
    })
  }

  try {
    // Se este veículo será o principal, remover status de principal dos outros
    if (is_primary) {
      const existingVehicles = await vehicleModuleService.listVehicles({
        customer_id: req.auth_context.customer_id
      })
      
      for (const vehicle of existingVehicles) {
        if (vehicle.is_primary) {
          await vehicleModuleService.updateVehicles(vehicle.id, {
            is_primary: false
          })
        }
      }
    }

    const vehicle = await vehicleModuleService.createVehicles({
      customer_id: req.auth_context.customer_id,
      plate: plate.toUpperCase(),
      brand,
      model,
      year,
      color,
      fuel,
      is_primary
    })

    return res.status(201).json({
      message: "Veículo adicionado com sucesso",
      vehicle: {
        id: vehicle.id,
        plate: vehicle.plate,
        brand: vehicle.brand,
        model: vehicle.model,
        year: vehicle.year,
        color: vehicle.color,
        fuel: vehicle.fuel,
        is_primary: vehicle.is_primary,
        created_at: vehicle.created_at
      }
    })

  } catch (error) {
    console.error("Erro ao adicionar veículo:", error)
    return res.status(500).json({
      message: "Erro ao adicionar veículo",
      error: error.message
    })
  }
}
  try {
    // Se este veículo será o principal, remover status de principal dos outros
    if (is_primary) {
      const existingVehicles = await vehicleModuleService.listVehicles({
        customer_id: req.auth_context.customer_id
      })
      
      for (const vehicle of existingVehicles) {
        if (vehicle.is_primary) {
          await vehicleModuleService.updateVehicles(vehicle.id, {
            is_primary: false
          })
        }
      }
    }

    const vehicle = await vehicleModuleService.createVehicles({
      customer_id: req.auth_context.customer_id,
      plate: plate.toUpperCase(),
      brand,
      model,
      year,
      color,
      fuel,
      is_primary
    })

    return res.status(201).json({
      message: "Veículo adicionado com sucesso",
      vehicle: {
        id: vehicle.id,
        plate: vehicle.plate,
        brand: vehicle.brand,
        model: vehicle.model,
        year: vehicle.year,
        color: vehicle.color,
        fuel: vehicle.fuel,
        is_primary: vehicle.is_primary,
        created_at: vehicle.created_at
      }
    })

  } catch (error) {
    console.error("Erro ao adicionar veículo:", error)
    return res.status(500).json({
      message: "Erro ao adicionar veículo",
      error: error.message
    })
  }
}
  
  const { marca, modelo, ano, placa, cor, km_atual, combustivel, observacoes } = req.body
  
  // Validação básica
  if (!marca || !modelo || !ano || !placa) {
    return res.status(400).json({
      message: "Marca, modelo, ano e placa são obrigatórios"
    })
  }
  
  // Criar veículo
  const vehicle = await vehicleModuleService.createVehicles({
    customer_id: customerId,
    marca,
    modelo,
    ano,
    placa,
    cor,
    km_atual,
    combustivel,
    observacoes
  })
  
  return res.status(201).json({ vehicle })
}


  try {
    // Se este veículo será o principal, remover status de principal dos outros
    if (is_primary) {
      const existingVehicles = await vehicleModuleService.listVehicles({
        customer_id: req.auth_context.customer_id
      })
      
      for (const vehicle of existingVehicles) {
        if (vehicle.is_primary) {
          await vehicleModuleService.updateVehicles(vehicle.id, {
            is_primary: false
          })
        }
      }
    }

    const vehicle = await vehicleModuleService.createVehicles({
      customer_id: req.auth_context.customer_id,
      plate: plate.toUpperCase(),
      brand,
      model,
      year,
      color,
      fuel,
      is_primary
    })

    return res.status(201).json({
      message: "Veículo adicionado com sucesso",
      vehicle: {
        id: vehicle.id,
        plate: vehicle.plate,
        brand: vehicle.brand,
        model: vehicle.model,
        year: vehicle.year,
        color: vehicle.color,
        fuel: vehicle.fuel,
        is_primary: vehicle.is_primary,
        created_at: vehicle.created_at
      }
    })

  } catch (error) {
    console.error("Erro ao adicionar veículo:", error)
    return res.status(500).json({
      message: "Erro ao adicionar veículo",
      error: error.message
    })
  }
}
  try {
    // Se este veículo será o principal, remover status de principal dos outros
    if (is_primary) {
      const existingVehicles = await vehicleModuleService.listVehicles({
        customer_id: req.auth_context.customer_id
      })
      
      for (const vehicle of existingVehicles) {
        if (vehicle.is_primary) {
          await vehicleModuleService.updateVehicles(vehicle.id, {
            is_primary: false
          })
        }
      }
    }

    const vehicle = await vehicleModuleService.createVehicles({
      customer_id: req.auth_context.customer_id,
      plate: plate.toUpperCase(),
      brand,
      model,
      year,
      color,
      fuel,
      is_primary
    })

    return res.status(201).json({
      message: "Veículo adicionado com sucesso",
      vehicle: {
        id: vehicle.id,
        plate: vehicle.plate,
        brand: vehicle.brand,
        model: vehicle.model,
        year: vehicle.year,
        color: vehicle.color,
        fuel: vehicle.fuel,
        is_primary: vehicle.is_primary,
        created_at: vehicle.created_at
      }
    })

  } catch (error) {
    console.error("Erro ao adicionar veículo:", error)
    return res.status(500).json({
      message: "Erro ao adicionar veículo",
      error: error.message
    })
  }
}
  
  const { marca, modelo, ano, placa, cor, km_atual, combustivel, observacoes } = req.body
  
  // Validação básica
  if (!marca || !modelo || !ano || !placa) {
    return res.status(400).json({
      message: "Marca, modelo, ano e placa são obrigatórios"
    })
  }
  
  // Criar veículo
  const vehicle = await vehicleModuleService.createVehicles({
    customer_id: customerId,
    marca,
    modelo,
    ano,
    placa,
    cor,
    km_atual,
    combustivel,
    observacoes
  })
  
  return res.status(201).json({ vehicle })
}

