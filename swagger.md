Checkout API
============
**Version:** v1

### /api/Cart/{cartId}
---
##### ***GET***
**Parameters**

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| cartId | path |  | Yes | string |

**Responses**

| Code | Description | Schema |
| ---- | ----------- | ------ |
| 200 | Success | [Cart](#cart) |
| 404 | Not Found |  |

### /api/Cart/{cartId}/clear
---
##### ***POST***
**Parameters**

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| cartId | path |  | Yes | string |

**Responses**

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 404 | Not Found |

### /api/Cart/{cartId}/setItem
---
##### ***POST***
**Parameters**

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| cartId | path |  | Yes | string |
| cartItem | body |  | No | [CartItem](#cartitem) |

**Responses**

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 404 | Not Found |

### /api/Cart/{cartId}/removeItem/{productId}
---
##### ***DELETE***
**Parameters**

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| cartId | path |  | Yes | string |
| productId | path |  | Yes | integer |

**Responses**

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 404 | Not Found |

### /api/Products/create
---
##### ***POST***
**Parameters**

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| product | body |  | No | [Product](#product) |

**Responses**

| Code | Description | Schema |
| ---- | ----------- | ------ |
| 201 | Success | [Product](#product) |
| 400 | Bad Request |  |

### /api/Products/update
---
##### ***POST***
**Parameters**

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| product | body |  | No | [Product](#product) |

**Responses**

| Code | Description | Schema |
| ---- | ----------- | ------ |
| 200 | Success | [Product](#product) |
| 400 | Bad Request |  |
| 404 | Not Found |  |

### /api/Products/delete/{id}
---
##### ***DELETE***
**Parameters**

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |
| id | path |  | Yes | integer |

**Responses**

| Code | Description |
| ---- | ----------- |
| 200 | Success |
| 400 | Bad Request |
| 404 | Not Found |

### /api/Products
---
##### ***GET***
**Parameters**

| Name | Located in | Description | Required | Schema |
| ---- | ---------- | ----------- | -------- | ---- |

**Responses**

| Code | Description | Schema |
| ---- | ----------- | ------ |
| 200 | Success | [ [Product](#product) ] |

### Models
---

### Cart  

| Name | Type | Description | Required |
| ---- | ---- | ----------- | -------- |
| id | string |  | No |
| cartItems | object |  | No |

### CartItem  

| Name | Type | Description | Required |
| ---- | ---- | ----------- | -------- |
| productId | integer |  | No |
| quantity | integer |  | No |

### Product  

| Name | Type | Description | Required |
| ---- | ---- | ----------- | -------- |
| id | integer |  | No |
| name | string |  | No |
| description | string |  | No |
| category | string |  | No |
| price | double |  | No |
| imageUrl | string |  | No |
| stock | integer |  | No |
| retained | integer |  | No |