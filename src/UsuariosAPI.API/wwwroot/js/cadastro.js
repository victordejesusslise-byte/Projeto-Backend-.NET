const formulario = document.querySelector("#form-cadastro");
const mensagem = document.querySelector("#mensagem");
const botao = document.querySelector("#botao-cadastrar");
const campoDataNascimento = document.querySelector("#dataNascimento");

campoDataNascimento.max = new Date().toISOString().split("T")[0];

function exibirMensagem(texto, tipo) {
  mensagem.textContent = texto;
  mensagem.className = `mensagem ${tipo}`;
  mensagem.hidden = false;
}

formulario.addEventListener("submit", async (evento) => {
  evento.preventDefault();
  mensagem.hidden = true;

  if (!formulario.reportValidity()) return;

  const dataNascimento = campoDataNascimento.value;
  const dados = {
    nome: document.querySelector("#nome").value.trim(),
    sobrenome: document.querySelector("#sobrenome").value.trim(),
    email: document.querySelector("#email").value.trim(),
    genero: document.querySelector("#genero").value,
    dataNascimento: dataNascimento || null
  };

  botao.disabled = true;
  botao.textContent = "Cadastrando...";

  try {
    const resposta = await fetch("/api/v1/usuarios", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(dados)
    });

    const conteudo = await resposta.json();

    if (!resposta.ok) {
      const detalhes = Array.isArray(conteudo.erros) && conteudo.erros.length
        ? conteudo.erros.join(" ")
        : conteudo.mensagem || "Não foi possível concluir o cadastro.";
      exibirMensagem(detalhes, "erro");
      return;
    }

    exibirMensagem("Cadastro realizado com sucesso!", "sucesso");
    formulario.reset();
  } catch {
    exibirMensagem("A API não está disponível. Tente novamente em instantes.", "erro");
  } finally {
    botao.disabled = false;
    botao.textContent = "Cadastrar";
  }
});
