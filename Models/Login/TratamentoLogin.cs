namespace NoVerdinho.Models.Login
{
    public class TratamentoLogin
    {


        public TratamentoLogin ValidarLogin(string email, string senha)
        {
            // Lógica de validação do login
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                throw new ArgumentException("Email e senha são obrigatórios.");
            }

            return new TratamentoLogin();
        }
    }
}