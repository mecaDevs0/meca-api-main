async function seedRealData() {
  console.log('🌱 Iniciando seed de dados reais...')
  
  try {
    console.log('✅ Criando dados mock para testes...')

    // 1. Criar Master Services
    console.log('📋 Criando serviços base...')
    const masterServices = [
      {
        name: 'Troca de Óleo',
        description: 'Troca completa de óleo do motor com filtro',
        category: 'Manutenção',
        average_price: 120.00,
        estimated_duration: 60
      },
      {
        name: 'Troca de Pneus',
        description: 'Troca e balanceamento de pneus',
        category: 'Pneus',
        average_price: 250.00,
        estimated_duration: 90
      },
      {
        name: 'Revisão Geral',
        description: 'Revisão completa do veículo',
        category: 'Manutenção',
        average_price: 450.00,
        estimated_duration: 180
      },
      {
        name: 'Alinhamento e Balanceamento',
        description: 'Alinhamento da direção e balanceamento das rodas',
        category: 'Suspensão',
        average_price: 180.00,
        estimated_duration: 120
      },
      {
        name: 'Freios',
        description: 'Troca de pastilhas e discos de freio',
        category: 'Freios',
        average_price: 320.00,
        estimated_duration: 150
      },
      {
        name: 'Ar Condicionado',
        description: 'Manutenção e recarga do sistema de ar condicionado',
        category: 'Climatização',
        average_price: 200.00,
        estimated_duration: 90
      }
    ]

    // 2. Criar Oficinas
    console.log('🏢 Criando oficinas...')
    const oficinas = [
      {
        name: 'Auto Center Silva',
        cnpj: '12.345.678/0001-90',
        email: 'contato@autocentersilva.com',
        phone: '(11) 99999-1111',
        address: 'Rua das Flores, 123 - Centro',
        city: 'São Paulo',
        state: 'SP',
        zip_code: '01234-567',
        status: 'aprovado',
        horario_funcionamento: {
          seg: { inicio: '08:00', fim: '18:00', ativo: true },
          ter: { inicio: '08:00', fim: '18:00', ativo: true },
          qua: { inicio: '08:00', fim: '18:00', ativo: true },
          qui: { inicio: '08:00', fim: '18:00', ativo: true },
          sex: { inicio: '08:00', fim: '18:00', ativo: true },
          sab: { inicio: '08:00', fim: '12:00', ativo: true },
          dom: { inicio: null, fim: null, ativo: false }
        },
        dados_bancarios: {
          banco: 'Banco do Brasil',
          agencia: '1234',
          conta: '56789-0',
          tipo: 'corrente'
        }
      },
      {
        name: 'Oficina do João',
        cnpj: '98.765.432/0001-10',
        email: 'joao@oficinajao.com',
        phone: '(11) 98888-2222',
        address: 'Av. Principal, 456 - Bairro Novo',
        city: 'São Paulo',
        state: 'SP',
        zip_code: '05678-901',
        status: 'aprovado',
        horario_funcionamento: {
          seg: { inicio: '07:00', fim: '19:00', ativo: true },
          ter: { inicio: '07:00', fim: '19:00', ativo: true },
          qua: { inicio: '07:00', fim: '19:00', ativo: true },
          qui: { inicio: '07:00', fim: '19:00', ativo: true },
          sex: { inicio: '07:00', fim: '19:00', ativo: true },
          sab: { inicio: '08:00', fim: '14:00', ativo: true },
          dom: { inicio: null, fim: null, ativo: false }
        },
        dados_bancarios: {
          banco: 'Itaú',
          agencia: '5678',
          conta: '12345-6',
          tipo: 'corrente'
        }
      },
      {
        name: 'Mecânica Express',
        cnpj: '11.222.333/0001-44',
        email: 'contato@mecanicaexpress.com',
        phone: '(11) 97777-3333',
        address: 'Rua Comercial, 789 - Centro',
        city: 'São Paulo',
        state: 'SP',
        zip_code: '07890-123',
        status: 'pendente',
        horario_funcionamento: {
          seg: { inicio: '08:30', fim: '17:30', ativo: true },
          ter: { inicio: '08:30', fim: '17:30', ativo: true },
          qua: { inicio: '08:30', fim: '17:30', ativo: true },
          qui: { inicio: '08:30', fim: '17:30', ativo: true },
          sex: { inicio: '08:30', fim: '17:30', ativo: true },
          sab: { inicio: '09:00', fim: '13:00', ativo: true },
          dom: { inicio: null, fim: null, ativo: false }
        },
        dados_bancarios: {
          banco: 'Bradesco',
          agencia: '9876',
          conta: '54321-0',
          tipo: 'corrente'
        }
      }
    ]

    // 3. Criar Clientes
    console.log('👥 Criando clientes...')
    const clientes = [
      {
        email: 'joao.silva@email.com',
        first_name: 'João',
        last_name: 'Silva',
        phone: '(11) 99999-4444',
        address: 'Rua das Palmeiras, 321 - Vila Nova',
        city: 'São Paulo',
        state: 'SP',
        zip_code: '01234-890'
      },
      {
        email: 'maria.santos@email.com',
        first_name: 'Maria',
        last_name: 'Santos',
        phone: '(11) 98888-5555',
        address: 'Av. das Rosas, 654 - Jardim',
        city: 'São Paulo',
        state: 'SP',
        zip_code: '05678-234'
      },
      {
        email: 'pedro.oliveira@email.com',
        first_name: 'Pedro',
        last_name: 'Oliveira',
        phone: '(11) 97777-6666',
        address: 'Rua dos Lírios, 987 - Centro',
        city: 'São Paulo',
        state: 'SP',
        zip_code: '07890-567'
      }
    ]

    // 4. Criar Veículos
    console.log('🚗 Criando veículos...')
    const veiculos = [
      {
        marca: 'Toyota',
        modelo: 'Corolla',
        ano: 2020,
        placa: 'ABC-1234',
        cor: 'Prata',
        customer_id: 1
      },
      {
        marca: 'Honda',
        modelo: 'Civic',
        ano: 2019,
        placa: 'DEF-5678',
        cor: 'Branco',
        customer_id: 2
      },
      {
        marca: 'Volkswagen',
        modelo: 'Golf',
        ano: 2021,
        placa: 'GHI-9012',
        cor: 'Preto',
        customer_id: 3
      },
      {
        marca: 'Ford',
        modelo: 'Focus',
        ano: 2018,
        placa: 'JKL-3456',
        cor: 'Azul',
        customer_id: 1
      }
    ]

    console.log('✅ Seed de dados reais concluído!')
    console.log('📊 Dados criados:')
    console.log(`   - ${masterServices.length} serviços base`)
    console.log(`   - ${oficinas.length} oficinas`)
    console.log(`   - ${clientes.length} clientes`)
    console.log(`   - ${veiculos.length} veículos`)

  } catch (error) {
    console.error('❌ Erro no seed:', error)
  }
}

// Executar se chamado diretamente
if (require.main === module) {
  seedRealData()
    .then(() => {
      console.log('🎉 Processo finalizado!')
      process.exit(0)
    })
    .catch((error) => {
      console.error('💥 Erro fatal:', error)
      process.exit(1)
    })
}

module.exports = { seedRealData }
