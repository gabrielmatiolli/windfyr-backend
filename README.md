#iniciar projeto e rodar migrations
dotnet ef database update --project Windfyr.Repository --startup-project Windfyr.WebApi; dotnet run --project Windfyr.WebApi

#iniciar 
dotnet run --project Windfyr.WebApi

#configurar .emv
oi matiolli se estiver vendo isso a chave da api da openwather é 4d3b254b321f69cb81f6e3bafc222dc4 (quando for ajustar o readme excluir isso)

#segurança
Regras de segurança implementadas: senha com mínimo de 6 dígitos, armazenamento criptografado de senha no banco de dados e validação de confirmação de senha para evitar erros de digitação e garantir maior segurança ao usuário.

#reamde
ajustes readme para explicar projeto, endpoints, segurança e testes implemnentados 