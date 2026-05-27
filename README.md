#iniciar projeto e rodar migrations
dotnet ef database update --project Windfyr.Repository --startup-project Windfyr.WebApi; dotnet run --project Windfyr.WebApi

#iniciar 
dotnet run --project Windfyr.WebApi

#configurar .env
oi matiolli se estiver vendo isso a chave da api da openwather é 4d3b254b321f69cb81f6e3bafc222dc4 (quando for ajustar o readme excluir isso)

#segurança
Regras de segurança implementadas: senha com mínimo de 6 dígitos, armazenamento criptografado de senha no banco de dados e validação de confirmação de senha para evitar erros de digitação e garantir maior segurança ao usuário.

#reamde
ajustes readme para explicar projeto, endpoints, segurança e testes implementados

#explicação 

O endpoint GET /api/lands é o ponto central da operação.

A cada requisição, ele faz três coisas de forma automática para cada terreno do usuário autenticado:

Atualiza dados climáticos:
consulta a OpenWeather com latitude/longitude do terreno para obter a condição mais recente.

Atualiza forecast:
calcula o risco de incêndio com base em temperatura, umidade e pressão.
Se os valores mudaram em relação ao último registro, salva um novo item em forecast_histories.

Atualiza irrigação:
a feature Irrigation avalia o risco calculado e define o estado do irrigador:

risco > 80%: ativado e via mqtt ativa também um dispositivo. 
risco <= 80%: desativado
Se o estado mudou em relação ao último histórico, salva em irrigation_histories