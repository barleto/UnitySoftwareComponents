### Implementação de componentes de software em unity:
# Unity Interface Outlets

##Descrição

## Racional de estudo
A IDE e motor de jogos Unity3D é muito famosa e muito usada, seja na industria de jogos (seu foco principal), mas também em outras áreas, como desenvolvimento de aplicativos mobile, já que seu suporte multiplataforma é bastante conciso e maduro. Infelizmente, na versão atual da plataforma (5.4.3), não existe uma forma padrão para a distribuição de software específico para Unity que seja de fácil integração e utilização, por não existir uma interface comum para a estrutura do esse tipo de software.

## Resultado
Com isso em mente e como parte de um estudo, foi desnvolvido um pequeno plugin para o editor Unity, chamado `Unity Interface Outlets`. Com o intúito de criar uma interface de _componentes de software_ que possa ser fácilmente integrada ao Unity, para facilitar a troca de código entre desenvolvedores melhorando a forma de como as partes do código se relacionam, aproveitando o melhor que o Unity tem a ofercer: O sistema de scripting de editor e o sistema de packaging do Unity3D, que nos permite criar extensões para adicionar novas funcionalidades e fácilmente instalá-las e compartilhá-las.

O plugin `Unity Interface Outlets`, não foi desenvolvido com o intuito de resolver sozinho as questões citadas no racional. Mas sim, fornecer um software que pretende ajudar a achar uma solução, provendo uma camada de abstração para a implementação do paradigma de componentes de software que, no estudo feito, pareceu ser um caminho viável para a solução desses problemas.

## Instalação do plugin
Para instalar o plugin de interface outlets, basta abrir o `Unity Package` que se encontra em [Interface Outlet Dwonload](https://github.com/barleto/UnitySoftwareComponents/raw/master/Assets/Unity%20Package/Unity_Interface_Outlet.unitypackage).

## Utilização

#### Criando uma interface de componente:
Primeiramente, crie a interface que servirá de protocolo para comunicação entre software e componente:


```c#
public interface ComponentInterface
{
    void anInterfaceMethod();
}
```

#### Adicionando um InterfaceOutlet à uma classe:

Em uma novo script `MonoBehaviour`(ou em um já existente), crie uma propriedade do tipo `UnityEngine.Object`, com o attributo `[InterfaceOutletAttribute(Type interfaceType)]`, como no script exemplo abaixo, `ObjectOutletTest.cs`:

```c#
using UnityEngine;
using System.Collections;

public class ObjectOutletTest : MonoBehaviour {

    //Outlet para componentes com interface do tipo ComponentInterface
    [InterfaceOutlet(typeof(ComponentInterface))]
    public UnityEngine.Object someComponent;

}
```
Perceba que passamos `typeof(ComponentInterface)` para o construtor do atributo `InterfaceOutlet`, assim definindo o tipo da interface que o componente deve implementar para ser posível plugá-lo à este outlet.
**O tipo da propriedade criada sempre deve ser `UnityEngine.Object`. Caso contrário, o comportamento é indefinido.**

#### Criando um componente:
A criação de componetes bastante direta: Basta criar um script `MonoBehaviour que implemente a interface de componente criada anteriormente.

Continuando nosso exemplo, temos o novo componente `ComponentScript.cs`:


```c#
using UnityEngine;
using System.Collections;
using System;

public class ComponentScript : MonoBehaviour, ComponentOutletInterface
{
    //implementação do método da interface
    public void interfaceMethod()
    {
        Debug.Log("Component attachment is working!");
    }
}

```

#### Plugando um componente em um Outlet:

Esse plugin usa as funcionalidades do editor do Unity para facilitar a integração entre software e seu componente.
Por causa do atributo `InterfaceOutlet`, no inspector do Unity, um novo tipo de propriedade irá aparecer, como no exemplo abaixo:

IMAGEM

Para integrar o componente basta arrastar um GameObject que tenha um `MonoBehaviour` que implemente a interface requerida. Com isso, a inetgração está feita.

#### Utilizando o componente:
Após a integração bem sucedida, é fácil utilizar o componente. Basta chamá-lo utlizando o seguinte tamplate:

```c#
if (component != null) {
    ((ComponentInterface)someComponent).anInterfaceMethod();
}
```

Obs: É necessário checar se a propriedade do componente está nula. Caso esteja, significa que nenhum componente foi plugado ao Outlet.

## Distribuíndo componentes
